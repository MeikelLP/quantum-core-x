using CommandLine;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands;

[Command("tphere", "Teleports you to a player")]
public class CommandTeleportHere : ICommandHandler<TeleportHereOptions>
{
    private readonly IWorld _world;

    public CommandTeleportHere(IWorld world)
    {
        _world = world;
    }

    public Task ExecuteAsync(CommandContext<TeleportHereOptions> ctx)
    {
        var target = _world.GetPlayer(ctx.Arguments.Target);

        if (target is null)
        {
            ctx.Player.SendChatInfo("Target not found");
        }
        else
        {
            ctx.Player.SendChatInfo($"Teleporting {target.Name} to your position");
            target.Move(ctx.Player.PositionX, ctx.Player.PositionY);
        }

        return Task.CompletedTask;
    }
}

public class TeleportHereOptions
{
    [Value(0)] public string Target { get; set; } = "";
}
