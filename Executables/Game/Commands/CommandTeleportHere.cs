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
    
    public async Task ExecuteAsync(CommandContext<TeleportHereOptions> ctx)
    {
        var target = _world.GetPlayer(ctx.Arguments.Target);

        if (target is null)
        {
            await ctx.Player.SendChatInfo("Target not found");
        }
        else
        {
            await ctx.Player.SendChatInfo($"Teleporting {target.Name} to your position");
            await target.Move(ctx.Player.PositionX, ctx.Player.PositionY);
        }
    }
}

public class TeleportHereOptions
{
    [Value(0)]
    public string Target { get; set; }
}