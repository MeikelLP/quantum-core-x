using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Dapper;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Cache;
using QuantumCore.Database;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.Commands
{
    public class CommandManager : ICommandManager
    {
        private readonly ILogger<CommandManager> _logger;
        private readonly IDatabaseManager _databaseManager;
        private readonly ICacheManager _cacheManager;
        private readonly IWorld _world;
        public Dictionary<string, CommandCache> Commands { get; } = new ();

        private readonly Dictionary<string, CommandDescriptor> _commandHandlers = new();
        public Dictionary<Guid, PermissionGroup> Groups { get; } = new ();

        public static readonly Guid Operator_Group = Guid.Parse("45bff707-1836-42b7-956d-00b9b69e0ee0");
        private readonly IServiceProvider _serviceProvider;

        public CommandManager(ILogger<CommandManager> logger, IDatabaseManager databaseManager, ICacheManager cacheManager, IWorld world, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _databaseManager = databaseManager;
            _cacheManager = cacheManager;
            _world = world;
            _serviceProvider = serviceProvider;
        }
        
        public void Register(string ns, Assembly assembly = null)
        {
            _logger.LogDebug("Registring commands from namespace {Namespace}", ns);
            if (assembly == null) assembly = Assembly.GetAssembly(typeof(CommandManager))!;
            
            var types = assembly.GetTypes().Where(t => string.Equals(t.Namespace, ns, StringComparison.Ordinal))
                .Where(t => (typeof(ICommandHandler<>).IsAssignableFrom(t) 
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
                Type optionsType = null;
                if (typeof(ICommandHandler<>).IsAssignableFrom(type))
                {
                    optionsType = type.GetInterfaces().FirstOrDefault(x => x == typeof(ICommandHandler<>));
                }
                _commandHandlers.Add(cmd, new CommandDescriptor(type, cmd, desc, optionsType, bypass));
            }
        }

        private async Task ParseGroup(Guid id, string name)
        {
            using var db = _databaseManager.GetGameDatabase();

            var p = new PermissionGroup
            {
                Id = id,
                Name = name,
                Users = new List<Guid>(),
                Permissions = new List<string>(),
            };

            if (id != Operator_Group)
            {
                var authq = await db.QueryAsync("SELECT Command FROM perm_auth WHERE `Group` = @Group", new { Group = id });

                foreach (var auth in authq)
                {
                    p.Permissions.Add(auth.Command);
                }
            }

            var pq = await db.QueryAsync("SELECT Player FROM perm_users WHERE `Group` = @Group", new { Group = id });

            foreach (var user in pq)
            {
                p.Users.Add(Guid.Parse(user.Player));

                var key = "perm:" + user.Player;
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
            using var db = _databaseManager.GetGameDatabase();

            var groups = await db.QueryAsync("SELECT * FROM perm_groups");

            foreach (var group in groups)
            {
                await ParseGroup(Guid.Parse(group.Id), group.Name);
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
            var args = CommandLineParser.SplitCommandLineIntoArguments(chatline, false).ToArray();
            var command = args[0][1..];

            if (_commandHandlers.TryGetValue(command, out var commandCache))
            {
                if (!CanUseCommand(connection.Player, command))
                {
                    await connection.Send(new ChatOutcoming()
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
                    var parserResult = Parser.Default.ParseArguments(args, _commandHandlers.Values.Select(x => x.Type).ToArray());
                    var methodInfo = typeof(ParserResultExtensions).GetMethod(nameof(ParserResultExtensions.MapResult),
                        new[] { typeof(Func<,>), typeof(Func<,>) })!;
                    var genericMethod = methodInfo.MakeGenericMethod(commandCache.Type, commandCache.OptionsType);
                    var successParam = Expression.Parameter(commandCache.OptionsType, "x");
                    var successExpression = Expression.Lambda(successParam, successParam);
                    var options =
                        genericMethod.Invoke(null, new object[] { parserResult, successExpression.Compile() });

                    var ctx = Activator.CreateInstance(typeof(CommandContext).MakeGenericType(commandCache.OptionsType),
                        new object[] {
                                connection.Player,
                                options
                        });
                    var cmdExecuteMethodInfo = typeof(ICommandHandler<>).MakeGenericType(commandCache.OptionsType)
                        .GetMethod(nameof(ICommandHandler<object>.ExecuteAsync))!;
                    var cmd = ActivatorUtilities.CreateInstance(_serviceProvider,
                        typeof(ICommandHandler<>).MakeGenericType(commandCache.OptionsType));

                    await (Task) cmdExecuteMethodInfo.Invoke(cmd, new object[] { ctx })!;
                }
                else
                {
                    var cmd = (ICommandHandler) ActivatorUtilities.CreateInstance(_serviceProvider, commandCache.Type);
                    await cmd.ExecuteAsync(new CommandContext(connection.Player));
                }
            }
            else
            {
                await connection.Send(new ChatOutcoming()
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
