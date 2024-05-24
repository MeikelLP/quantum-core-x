using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

[PacketHandler(typeof(ChatIncoming))]
public class ChatIncomingHandler
{
    private readonly ICommandManager _commandManager;
    private readonly IChatManager _chatManager;
    private readonly ILogger<ChatIncomingHandler> _logger;

    public ChatIncomingHandler(ICommandManager commandManager, IChatManager chatManager,
        ILogger<ChatIncomingHandler> logger)
    {
        _commandManager = commandManager;
        _chatManager = chatManager;
        _logger = logger;
    }

    public void Execute(GamePacketContext ctx, ChatIncoming packet)
    {
        if (ctx.Connection.Player is null)
        {
            _logger.LogCritical("Player is not set on connection. This must never happen");
            ctx.Connection.Close();
            return;
        }

        if (packet.MessageType == ChatMessageTypes.Normal)
        {
            if (packet.Message.StartsWith('/'))
            {
                await _commandManager.Handle(ctx.Connection, packet.Message);
            }
            else
            {
                var message = ctx.Connection.Player.Name + ": " + packet.Message;

                _chatManager.Talk(ctx.Connection.Player, message);
            }
        }

        if (packet.MessageType == ChatMessageTypes.Shout)
        {
            // todo check 15 seconds cooldown
            var message = ctx.Connection.Player.Name + ": " + packet.Message;

            await _chatManager.Shout(message);
        }
    }
}