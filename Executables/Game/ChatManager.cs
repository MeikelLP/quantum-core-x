using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Caching;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game;

public class ChatManager : IChatManager
{
    private readonly ICacheManager _cacheManager;

    private struct ChatMessage
    {
        public Guid OwnerCore { get; set; }
        public ChatMessageTypes Type { get; set; }
        public string Message { get; set; }
    }

    private IRedisSubscriber _subscriber;
    private Guid _id;

    public ChatManager(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }
    
    public void Init()
    {
        _id = Guid.NewGuid();

        _subscriber = _cacheManager.Subscribe();
#pragma warning disable CS4014
        _subscriber.Register<ChatMessage>("chat", msg => OnChatMessage(msg));
#pragma warning restore CS4014
        _subscriber.Listen();
    }

    private async Task OnChatMessage(ChatMessage message)
    {
        if (message.OwnerCore == _id)
        {
            // It's our own message, we don't have to handle it here
            return;
        }
        
        var chat = new ChatOutcoming
        {
            MessageType = message.Type,
            Vid = 0,
            Empire = 1, // todo
            Message = message.Message
        };
        
        // Send message to all connections in the game phase
        await GameServer.Instance.ForAllConnections(async connection =>
        {
            if (connection.Phase != EPhases.Game)
            {
                return;
            }
            
            await connection.Send(chat);
        });
    }
    
    public async ValueTask Talk(IEntity entity, string message)
    {
        var packet = new ChatOutcoming
        {
            MessageType = ChatMessageTypes.Normal,
            Vid = entity.Vid,
            Empire = entity.Empire,
            Message = message
        };

        if (entity is IPlayerEntity player)
        {
            await player.Connection.Send(packet);
        }
        
        await entity.ForEachNearbyEntity(async nearby =>
        {
            if (nearby is PlayerEntity player)
            {
                await player.Connection.Send(packet);
            }
        });
    }

    public async Task Shout(string message)
    {
        var chat = new ChatOutcoming
        {
            MessageType = ChatMessageTypes.Shout,
            Vid = 0,
            Empire = 1, // todo
            Message = message
        };
        
        // Send message to all connections in the game phase
        await GameServer.Instance.ForAllConnections(async connection =>
        {
            if (connection.Phase != EPhases.Game)
            {
                return;
            }
            
            await connection.Send(chat);
        });
        
        // Broadcast message to all cores 
        await _cacheManager.Publish("chat",
            new ChatMessage {Type = ChatMessageTypes.Shout, Message = message, OwnerCore = _id});
    }
}