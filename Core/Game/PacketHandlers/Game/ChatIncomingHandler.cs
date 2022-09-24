using System.Threading;
using System.Threading.Tasks;
using QuantumCore.API;
using QuantumCore.Core.Networking;
using QuantumCore.Game.Commands;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ChatIncomingHandler : IPacketHandler<ChatIncoming>
{
    private readonly ICommandManager _commandManager;
    private readonly IChatManager _chatManager;

    public ChatIncomingHandler(ICommandManager commandManager, IChatManager chatManager)
    {
        _commandManager = commandManager;
        _chatManager = chatManager;
    }
        
    public async Task ExecuteAsync(PacketContext<ChatIncoming> ctx, CancellationToken token = default)
    {
        if (ctx.Packet.MessageType == ChatMessageTypes.Normal)
        {
            if (ctx.Packet.Message.StartsWith('/'))
            {
                await _commandManager.Handle(ctx.Connection, ctx.Packet.Message);
            }
            else
            {
                var message = ctx.Connection.Player.Name + ": " + ctx.Packet.Message;

                await _chatManager.Talk(ctx.Connection.Player, message);
            }
        }

        if (ctx.Packet.MessageType == ChatMessageTypes.Shout)
        {
            // todo check 15 seconds cooldown
            var message = ctx.Connection.Player.Name + ": " + ctx.Packet.Message;
                
            await _chatManager.Shout(message);
        }
    }
}