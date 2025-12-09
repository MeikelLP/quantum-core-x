using EnumsNET;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ChatIncomingHandler : IGamePacketHandler<ChatIncoming>
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

    public async Task ExecuteAsync(GamePacketContext<ChatIncoming> ctx, CancellationToken token = default)
    {
        if (ctx.Connection.Player is null)
        {
            _logger.LogCritical("Player is not set on connection. This must never happen");
            ctx.Connection.Close();
            return;
        }

        if (!Enums.TryToObject<ChatMessageType>(ctx.Packet.MessageType, out var messageType, EnumValidation.IsDefined))
        {
            _logger.LogWarning("Undefined {ChatMessageType} {MessageType}, ignoring packet",
                nameof(ChatMessageType), ctx.Packet.MessageType);
            return;
        }

        if (messageType == ChatMessageType.Normal)
        {
            if (ctx.Packet.Message.StartsWith('/'))
            {
                await _commandManager.Handle(ctx.Connection, ctx.Packet.Message);
            }
            else
            {
                var message = ctx.Connection.Player.Name + ": " + ctx.Packet.Message;

                _chatManager.Talk(ctx.Connection.Player, message);
            }
        }

        if (messageType == ChatMessageType.Shout)
        {
            // todo check 15 seconds cooldown
            var message = ctx.Connection.Player.Name + ": " + ctx.Packet.Message;

            await _chatManager.Shout(message);
        }
    }
}
