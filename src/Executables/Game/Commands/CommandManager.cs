using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using CommandLine;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Caching;
using QuantumCore.Game.Packets;
using QuantumCore.Game.Persistence;

namespace QuantumCore.Game.Commands
{
    public class CommandManager : ICommandManager
    {
        private readonly ILogger<CommandManager> _logger;
        private readonly ICacheManager _cacheManager;
        private readonly Dictionary<string, CommandDescriptor> _commandHandlers = new();
        public Dictionary<Guid, PermissionGroup> Groups { get; } = new ();

        public static readonly Guid Operator_Group = Guid.Parse("45bff707-1836-42b7-956d-00b9b69e0ee0");
        private readonly IServiceProvider _serviceProvider;
        private readonly ICommandPermissionRepository _repository;

        private static readonly Parser ParserInstance = new Parser(settings =>
            {
                settings.CaseInsensitiveEnumValues = true;
            }
        );

        public CommandManager(ILogger<CommandManager> logger, ICacheManager cacheManager, IWorld world,
            IServiceProvider serviceProvider, ICommandPermissionRepository repository)
        {
            _logger = logger;
            _cacheManager = cacheManager;
            _serviceProvider = serviceProvider;
            _repository = repository;
        }

        public void Register(string ns, Assembly? assembly = null)
        {
            _logger.LogDebug("Registring commands from namespace {Namespace}", ns);
            if (assembly == null) assembly = Assembly.GetAssembly(typeof(CommandManager))!;

            var types = assembly.GetTypes().Where(t => string.Equals(t.Namespace, ns, StringComparison.Ordinal))
                .Where(t => (t.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommandHandler<>))
                             || typeof(ICommandHandler).IsAssignableFrom(t)) && t.IsClass && !t.IsAbstract)
                .ToArray();

            foreach (var type in types)
            {
                var cmdAttr = type.GetCustomAttribute<CommandAttribute>();
                if (cmdAttr is null)
                {
                    _logger.LogWarning("Command handler {Type} is implementing {HandlerInterface} but is missing a {AttributeName}", type.Name, nameof(ICommandHandler), nameof(CommandAttribute));
                    continue;
                }
                var cmd = cmdAttr.Name;
                var desc = cmdAttr.Description;
                var bypass = type.GetCustomAttribute<CommandNoPermissionAttribute>() is not null;
                Type? optionsType = null;
                var intf = type.GetInterfaces().FirstOrDefault(x =>
                    x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommandHandler<>));
                if (intf is not null)
                {
                    optionsType = intf.GenericTypeArguments[0];
                }
                _commandHandlers.Add(cmd, new CommandDescriptor(type, cmd, desc, optionsType, bypass));
            }
        }

        private async Task ParseGroup(Guid id, string name)
        {
            var p = new PermissionGroup
            {
                Id = id,
                Name = name,
                Users = new List<Guid>(),
                Permissions = new List<string>(),
            };

            if (id != Operator_Group)
            {
                var authq = await _repository.GetPermissionsForGroupAsync(id);

                foreach (var command in authq)
                {
                    p.Permissions.Add(command);
                }
            }

            var pq = await _repository.GetPlayerIdsInGroupAsync(id);

            foreach (var userId in pq)
            {
                p.Users.Add(userId);

                var key = "perm:" + userId;
                var redisList = _cacheManager.CreateList<Guid>(key);

                await redisList.Push(id);
            }

            Groups.Add(p.Id, p);
        }

        public async Task LoadAsync(CancellationToken token = default)
        {
            _logger.LogInformation("Initialize permissions");
            var permission_keys = await _cacheManager.Keys("perm:*");

            foreach (var p in permission_keys)
            {
                await _cacheManager.Del(p);
            }

            var groups = await _repository.GetGroupsAsync();

            foreach (var group in groups)
            {
                await ParseGroup(group.Id, group.Name);
            }

            await ParseGroup(Operator_Group, "Operator");
        }

        public bool HavePerm(Guid group, string cmd)
        {
            if (!Groups.ContainsKey(group))
            {
                return false;
            }

            var g = Groups[group];

            foreach (var p in g.Permissions)
            {
                if (p == cmd)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanUseCommand(IPlayerEntity player, string cmd)
        {
            if (_commandHandlers[cmd].BypassPerm)
            {
                return true;
            }

            foreach (var group in player.Groups)
            {
                if (group == Operator_Group)
                {
                    return true;
                }
                else if (HavePerm(group, cmd))
                {
                    return true;
                }

            }

            return false;
        }

        public async Task Handle(IGameConnection connection, string chatline)
        {
            if (connection.Player is null)
            {
                _logger.LogCritical("Cannot handle command if connection's player is null. This should never happen");
                return;
            }
            var args = CommandLineParser.SplitCommandLineIntoArguments(chatline.TrimStart('/'), false).ToArray();
            var command = args[0];
            var argsWithoutCommand = args.Skip(1).ToArray();

            if (command.Equals("help", StringComparison.InvariantCultureIgnoreCase))
            {
                // special case for help

                var sb = new StringBuilder("The following commands are available:\n");
                foreach (var handler in _commandHandlers)
                {
                    sb.AppendLine($"- /{handler.Key}");
                }

                var msg = sb.ToString();

                connection.Player.SendChatMessage(msg);
            }
            else
            {
                if (_commandHandlers.TryGetValue(command, out var commandCache))
                {
                    if (!CanUseCommand(connection.Player, command))
                    {
                        connection.Send(new ChatOutcoming()
                        {
                            MessageType = ChatMessageTypes.Info,
                            Vid = 0,
                            Empire = 0,
                            Message = $"You don't have enough permission to use this command"
                        });
                        return;
                    }

                    if (commandCache.OptionsType is not null)
                    {
                        var parserMethod = typeof(Parser)
                            .GetMethods()
                            .Single(x => x.Name == nameof(Parser.ParseArguments) && x.GetParameters().Length == 1)
                            .MakeGenericMethod(commandCache.OptionsType);

                        // basically makes a ICommandHandler<TCommandOptions> for the given command
                        // creates a context with the given CommandContext<TCommandContext>
                        // invokes the command
                        // this may be improved in the future (caching)

                        var parserResult = parserMethod.Invoke(ParserInstance, new object [] { argsWithoutCommand })!;
                        var methodInfo = typeof(ParserResultExtensions).GetMethods().Single(x =>
                        {
                            var nameMatches = x.Name == nameof(ParserResultExtensions.MapResult);
                            if (!nameMatches) return false; // return early
                            var parameters = x.GetParameters();
                            if (parameters.Length != 3) return false; // return early
                            var param1 = parameters[0].ParameterType;
                            var param2 = parameters[1].ParameterType;
                            var param3 = parameters[2].ParameterType;
                            return param1.IsGenericType &&
                                   param1.GetGenericTypeDefinition() == typeof(ParserResult<>) &&
                                   param1.GenericTypeArguments[0].IsGenericParameter &&

                                   param2.IsGenericType &&
                                   param2.GetGenericTypeDefinition() == typeof(Func<,>) &&

                                   param3.IsGenericType &&
                                   param3.GetGenericTypeDefinition() == typeof(Func<,>) &&
                                   param3.GenericTypeArguments[0] == typeof(IEnumerable<Error>);
                        })!;
                        var genericMethod = methodInfo.MakeGenericMethod(commandCache.OptionsType, commandCache.OptionsType);
                        var successParam = Expression.Parameter(commandCache.OptionsType, "x");
                        var successExpression = Expression.Lambda(successParam, successParam);
                        var errorParam = Expression.Parameter(typeof(IEnumerable<Error>), "x");
                        var errorConstant = Expression.Constant(Activator.CreateInstance(commandCache.OptionsType));
                        var errorExpression = Expression.Lambda(errorConstant, errorParam);
                        var options =
                            genericMethod.Invoke(null, new object[] { parserResult, successExpression.Compile(), errorExpression.Compile() })!;

                        var ctx = Activator.CreateInstance(typeof(CommandContext<>).MakeGenericType(commandCache.OptionsType),
                            new object[] {
                                    connection.Player,
                                    options
                            })!;
                        var cmdExecuteMethodInfo = typeof(ICommandHandler<>).MakeGenericType(commandCache.OptionsType)
                            .GetMethod(nameof(ICommandHandler<object>.ExecuteAsync))!;
                        var cmd = ActivatorUtilities.CreateInstance(_serviceProvider, commandCache.Type);

                        try
                        {
                            await (Task) cmdExecuteMethodInfo.Invoke(cmd, new object[] { ctx })!;
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, "Failed to execute command {Type}!", commandCache.Type.Name);
                            connection.Send(new ChatOutcoming()
                            {
                                MessageType = ChatMessageTypes.Info,
                                Vid = 0,
                                Empire = 0,
                                Message = $"Failed to execute command {command}"
                            });
                        }
                    }
                    else
                    {
                        var cmd = (ICommandHandler) ActivatorUtilities.CreateInstance(_serviceProvider, commandCache.Type);
                        await cmd.ExecuteAsync(new CommandContext(connection.Player));
                    }
                }
                else
                {
                    connection.Send(new ChatOutcoming()
                    {
                        MessageType = ChatMessageTypes.Info,
                        Vid = 0,
                        Empire = 0,
                        Message = $"Unknown command {command}"
                    });
                }
            }
        }
    }
}
