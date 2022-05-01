using System;
using BeetleX.Redis;
using QuantumCore.API.Game.World;
using QuantumCore.Cache;
using QuantumCore.Core.Constants;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.Entities;
using Serilog;

namespace QuantumCore.Game;

public static class ChatManager
{
    private struct ChatMessage
    {
        public Guid OwnerCore { get; set; }
        public ChatMessageTypes Type { get; set; }
        public string Message { get; set; }
    }

    private static Subscriber _subscriber;
    private static Guid _id;

    public static void Init()
    {
        _id = Guid.NewGuid();

        _subscriber = CacheManager.Redis.Subscribe();
        _subscriber.Register<ChatMessage>("chat", OnChatMessage);
        _subscriber.Listen();
    }

    private static void OnChatMessage(ChatMessage message)
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
        GameServer.Instance.Server.ForAllConnections(connection =>
        {
            if (connection.Phase != EPhases.Game)
            {
                return;
            }
            
            connection.Send(chat);
        });
    }
    
    public static void Talk(IEntity entity, string message)
    {
        var packet = new ChatOutcoming
        {
            MessageType = ChatMessageTypes.Normal,
            Vid = entity.Vid,
            Empire = 1, // todo
            Message = message
        };

        if (entity is IPlayerEntity player)
        {
            player.Connection.Send(packet);
        }
        
        entity.ForEachNearbyEntity(nearby =>
        {
            if (nearby is PlayerEntity player)
            {
                player.Connection.Send(packet);
            }
        });
    }

    public static async void Shout(string message)
    {
        var chat = new ChatOutcoming
        {
            MessageType = ChatMessageTypes.Shout,
            Vid = 0,
            Empire = 1, // todo
            Message = message
        };
        
        // Send message to all connections in the game phase
        GameServer.Instance.Server.ForAllConnections(connection =>
        {
            if (connection.Phase != EPhases.Game)
            {
                return;
            }
            
            connection.Send(chat);
        });
        
        // Broadcast message to all cores 
        await CacheManager.Redis.Publish("chat",
            new ChatMessage {Type = ChatMessageTypes.Shout, Message = message, OwnerCore = _id});
    }
}