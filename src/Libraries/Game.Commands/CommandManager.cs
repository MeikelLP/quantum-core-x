using System.Linq.Expressions;
using System.Reflection;
using CommandLine;
using CommandLine.Text;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuantumCore.API;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Caching;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.Persistence.Entities;

namespace QuantumCore.Game.Commands;

internal class CommandManager : ICommandManager, ILoadable
{
    private readonly ILogger<CommandManager> _logger;
    private readonly ICacheManager _cacheManager;
    private readonly SortedDictionary<string, CommandDescriptor> _commandHandlers = new();
    public Dictionary<Guid, PermissionGroup> Groups { get; } = new();

    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<GameCommandOptions> _options;

    private static readonly Parser ParserInstance = new Parser(settings =>
        {
            settings.CaseInsensitiveEnumValues = true;
            settings.HelpWriter = null;
        }
    );

    public CommandManager(ILogger<CommandManager> logger, ICacheManager cacheManager,
        IServiceProvider serviceProvider, IOptions<GameCommandOptions> options)
    {
        _logger = logger;
        _cacheManager = cacheManager;
        _serviceProvider = serviceProvider;
        _options = options;
    }

    public void Register(string ns, Assembly? assembly = null)
    {
        _logger.LogDebug("Registring commands from namespace {Namespace}", ns);
        if (assembly == null) assembly = Assembly.GetAssembly(typeof(CommandManager))!;

        var types = assembly.GetTypes().Where(t => string.Equals(t.Namespace, ns, StringComparison.Ordinal))
            .Where(t => (t.GetInterfaces().Any(x =>
                             x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommandHandler<>))
                         || typeof(ICommandHandler).IsAssignableFrom(t)) && t.IsClass && !t.IsAbstract)
            .ToArray();

        foreach (var type in types)
        {
            var cmdAttrs = type.GetCustomAttributes<CommandAttribute>().ToList();
            if (cmdAttrs.Count > 0)
            {
                foreach (var cmdAttr in cmdAttrs)
                {
                    ProcessCommandAttribute(type, cmdAttr);
                }
            }
            else
            {
                _logger.LogWarning("Command {Type} does not have a CommandAttribute", type.Name);
            }
        }

        void ProcessCommandAttribute(Type type, CommandAttribute? cmdAttr)
        {
            if (cmdAttr is null)
            {
                _logger.LogWarning(
                    "Command handler {Type} is implementing {HandlerInterface} but is missing a {AttributeName}",
                    type.Name, nameof(ICommandHandler), nameof(CommandAttribute));
                return;
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

    public async Task LoadAsync(CancellationToken token = default)
    {
        _logger.LogInformation("Initialize permissions");
        var permissionKeys = await _cacheManager.Keys("perm:*");

        foreach (var p in permissionKeys)
        {
            await _cacheManager.Del(p);
        }

        await using (var scope = _serviceProvider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<ICommandPermissionRepository>();
            var groups = await repository.GetGroupsAsync();
            Groups.Clear();
            foreach (var group in groups)
            {
                Groups.Add(group.Id, group);
            }
        }
    }

    public async Task ReloadAsync(CancellationToken token = default)
    {
        Groups.Clear();
        await LoadAsync(token);
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
            if (group == PermGroup.OperatorGroup)
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

            connection.Player.SendChatMessage("The following commands are available:");
            foreach (var handler in _commandHandlers)
            {
                connection.Player.SendChatInfo($"- /{handler.Key}");
            }
        }
        else
        {
            if (_commandHandlers.TryGetValue(command, out var commandCache))
            {
                if (!CanUseCommand(connection.Player, command))
                {
                    connection.Player.SendChatInfo("You don't have enough permission to use this command");
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

                    var parserResult = parserMethod.Invoke(ParserInstance, [argsWithoutCommand])!;

                    if (parserResult.GetType().IsGenericType &&
                        parserResult.GetType().GetGenericTypeDefinition() == typeof(NotParsed<>))
                    {
                        var resultType = typeof(ParserResult<>).MakeGenericType(commandCache.OptionsType);
                        var errors =
                            (IEnumerable<Error>)resultType.GetProperty(nameof(ParserResult<object>.Errors))!.GetValue(
                                parserResult)!;

                        if (_options.Value.StrictMode)
                        {
                            throw new CommandValidationException(command)
                            {
                                Errors = [..errors.Select(e => e.GetType().Name)]
                            };
                        }
                        else
                        {
                            Func<HelpText, HelpText> helpTextFunc = h =>
                            {
                                h.Copyright = "";
                                h.AutoVersion = false;
                                h.AutoHelp = false;
                                h.Heading = "";
                                return h;
                            };
                            Func<Example, Example> exampleFunc = example => example;
                            var verbsIndex = false;
                            var maxDisplayWidth = 80;
                            var helpTextMethod = typeof(HelpText)
                                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                                .First(x => x.Name == nameof(HelpText.AutoBuild) && x.GetParameters().Length == 5)!
                                .MakeGenericMethod(commandCache.OptionsType);
                            var help = (HelpText)helpTextMethod.Invoke(null,
                                [parserResult, helpTextFunc, exampleFunc, verbsIndex, maxDisplayWidth])!;
                            var messages = help.ToString().Split(Environment.NewLine)
                                .Where(x => !string.IsNullOrWhiteSpace(x));
                            connection.Player.SendChatInfo("Comannd validation failed:");
                            foreach (var message in messages)
                            {
                                connection.Player.SendChatInfo(message);
                            }

                            return;
                        }
                    }

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
                    var genericMethod =
                        methodInfo.MakeGenericMethod(commandCache.OptionsType, commandCache.OptionsType);
                    var successParam = Expression.Parameter(commandCache.OptionsType, "x");
                    var successExpression = Expression.Lambda(successParam, successParam);
                    var errorParam = Expression.Parameter(typeof(IEnumerable<Error>), "x");
                    var errorConstant = Expression.Constant(Activator.CreateInstance(commandCache.OptionsType));
                    var errorExpression = Expression.Lambda(errorConstant, errorParam);
                    var options =
                        genericMethod.Invoke(null,
                            new object[] {parserResult, successExpression.Compile(), errorExpression.Compile()})!;

                    var ctx = Activator.CreateInstance(
                        typeof(CommandContext<>).MakeGenericType(commandCache.OptionsType),
                        new object[] {connection.Player, options})!;
                    var cmdExecuteMethodInfo = typeof(ICommandHandler<>).MakeGenericType(commandCache.OptionsType)
                        .GetMethod(nameof(ICommandHandler<object>.ExecuteAsync))!;
                    await using var scope = _serviceProvider.CreateAsyncScope();
                    var cmd = ActivatorUtilities.CreateInstance(scope.ServiceProvider, commandCache.Type);

                    try
                    {
                        await (Task)cmdExecuteMethodInfo.Invoke(cmd, new object[] {ctx})!;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to execute command {Type}!", commandCache.Type.Name);
                        connection.Player.SendChatInfo($"Failed to execute command {command}");
                    }
                }
                else
                {
                    await using var scope = _serviceProvider.CreateAsyncScope();
                    var cmd = (ICommandHandler)ActivatorUtilities.CreateInstance(scope.ServiceProvider,
                        commandCache.Type);
                    await cmd.ExecuteAsync(new CommandContext(connection.Player));
                }
            }
            else if (_options.Value.StrictMode)
            {
                throw new CommandHandlerNotFoundException(command);
            }
            else
            {
                connection.Player.SendChatInfo($"Unknown command {command}");
            }
        }
    }
}
