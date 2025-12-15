using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Caching;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game;

public class ChatManager : IChatManager, ILoadable
{
    private readonly IRedisStore _cacheManager;

    private struct ChatMessage
    {
        public Guid OwnerCore { get; set; }
        public ChatMessageType Type { get; set; }
        public string Message { get; set; }
    }

    private IRedisSubscriber? _subscriber;
    private Guid _id;

    public ChatManager(ICacheManager cacheManager)
    {
        _cacheManager = cacheManager.Server;
    }


    public Task LoadAsync(CancellationToken token = default)
    {
        _id = Guid.NewGuid();

        _subscriber = _cacheManager.Subscribe();
        _subscriber.Register<ChatMessage>("chat", OnChatMessage);
        _subscriber.Listen();


        return Task.CompletedTask;
    }

    private void OnChatMessage(ChatMessage message)
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
            Empire = EEmpire.SHINSOO, // todo
            Message = message.Message
        };

        // Send message to all connections in the game phase
        GameServer.Instance.ForAllConnections(connection =>
        {
            if (connection.Phase != EPhase.GAME)
            {
                return;
            }

            connection.Send(chat);
        });
    }

    public void Talk(IEntity entity, string message)
    {
        var packet = new ChatOutcoming
        {
            MessageType = ChatMessageType.NORMAL, Vid = entity.Vid, Empire = entity.Empire, Message = message
        };

        if (entity is IPlayerEntity player)
        {
            player.Connection.Send(packet);
        }

        foreach (var nearby in entity.NearbyEntities)
        {
            if (nearby is PlayerEntity p)
            {
                p.Connection.Send(packet);
            }
        }
    }

    public async Task Shout(string message)
    {
        var chat = new ChatOutcoming
        {
            MessageType = ChatMessageType.SHOUT,
            Vid = 0,
            Empire = EEmpire.SHINSOO, // todo
            Message = message
        };

        // Send message to all connections in the game phase
        GameServer.Instance.ForAllConnections(connection =>
        {
            if (connection.Phase != EPhase.GAME)
            {
                return;
            }

            connection.Send(chat);
        });

        // Broadcast message to all cores
        await _cacheManager.Publish("chat",
            new ChatMessage {Type = ChatMessageType.SHOUT, Message = message, OwnerCore = _id});
    }
}
