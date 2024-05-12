using CommandLine;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands;

[Command("reload_perms", "Reloads the permissions of the target player or all players if no target is specified")]
[CommandNoPermission]
public class ReloadPermissionsCommand : ICommandHandler<ReloadPermissionsCommandOptions>
{
    private readonly IWorld _world;
    private readonly ICommandManager _commandManager;

    public ReloadPermissionsCommand(IWorld world, ICommandManager commandManager)
    {
        _world = world;
        _commandManager = commandManager;
    }
    
    public async Task ExecuteAsync(CommandContext<ReloadPermissionsCommandOptions> context)
    {
        // Reload in-memory + cache permissions
        await _commandManager.ReloadAsync();
        
        var target = string.Equals(context.Arguments.Target, "$self", StringComparison.InvariantCultureIgnoreCase)
            ? context.Player
            : _world.GetPlayer(context.Arguments.Target);
        
        // Reload permissions for target player or all players
        if (target is not null)
        {
            await target.ReloadPermissions();
        }
        else
        {
            var players = _world.GetPlayers();
            await Task.WhenAll(players.Select(x => x.ReloadPermissions()));
        }
        
        context.Player.SendChatInfo("Permissions reloaded");
    }
}

public class ReloadPermissionsCommandOptions
{
    [Value(0)] public string Target { get; set; }
}
