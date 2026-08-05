using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Packets;
using QuantumCore.API.PluginTypes;

namespace QuantumCore.Game.PacketHandlers.Game;

public class WhisperHandler : IGamePacketHandler<WhisperIncoming>
{
    private readonly ILogger<WhisperHandler> _logger;

    public WhisperHandler(ILogger<WhisperHandler> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(GamePacketContext<WhisperIncoming> ctx, CancellationToken token = default)
    {
        var sender = ctx.Connection.Player;
        if (sender is null) return Task.CompletedTask;

        var target = sender.Map!.World.GetPlayers()
            .FirstOrDefault(p => p.Name == ctx.Packet.NameTo);

        if (target is null)
        {
            sender.Connection.Send(new WhisperOutcoming
            {
                Type = WhisperType.UNKNOWN_RECIPIENT,
                NameFrom = ctx.Packet.NameTo,
                Message = ""
            });
            _logger.LogDebug("Player tried to send a message to {Target}, but it does not exist", ctx.Packet.NameTo);
            return Task.CompletedTask;
        }

        target.Connection.Send(new WhisperOutcoming
        {
            Type = WhisperType.CHAT,
            NameFrom = sender.Name,
            Message = ctx.Packet.Message
        });
        _logger.LogDebug("Player sent a message to {Target}", ctx.Packet.NameTo);

        return Task.CompletedTask;
    }
}