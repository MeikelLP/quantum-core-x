using CommandLine;
using QuantumCore.API.Game;
using QuantumCore.API.Packets;
using QuantumCore.Game.Extensions;

namespace QuantumCore.Game.Commands;

[Command("motion", "Play a motion animation on your character")]
public class MotionCommand : ICommandHandler<MotionOptions>
{
    public Task ExecuteAsync(CommandContext<MotionOptions> ctx)
    {
        var player = ctx.Player;
        var packet = new Motion
        {
            Vid = player.Vid,
            VictimVid = 0,
            MotionId = ctx.Arguments.MotionId
        };

        player.Connection.Send(packet);
        player.SendChatCommand($"dance1 {player.Vid} 0");
        foreach (var entity in player.NearbyPlayers)
        {
            entity.Connection.Send(packet);
        }

        return Task.CompletedTask;
    }
}

public class MotionOptions
{
    [Value(0, Required = true)] public ushort MotionId { get; set; }
}