using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.Caching;
using QuantumCore.Game.Packets;
using QuantumCore.Game.Packets.General;
using QuantumCore.Game.Packets.Mall;

namespace QuantumCore.Game.PlayerUtils;

public class Mall : IMall
{
    private readonly ILogger<Mall> _logger;
    private readonly IConnection _connection;
    private readonly IItemManager _itemManager;
    private readonly ICacheManager _cacheManager;
    public DateTime? LastInteraction { get; private set; }

    public Mall(ILogger<Mall> logger, IConnection connection, IItemManager itemManager, ICacheManager cacheManager)
    {
        _logger = logger;
        _connection = connection;
        _itemManager = itemManager;
        _cacheManager = cacheManager;
        LastInteraction = null;
    }

    public Task Load()
    {
        // todo load items from the mall (cache/db)
        return Task.CompletedTask;
    }

    public void PromptPassword()
    {
        SendChatCommand("ShowMeMallPassword");
    }

    public void Open()
    {
        LastInteraction = DateTime.UtcNow;
        // todo check already other trade windows opened
        // todo magic number (3tabs * 9slots = 27)
        _connection.Send(new MallboxSize { Size = 27 }); 
    }

    public void SendItems()
    {
        var proto = _itemManager.GetItem(11210)!;
        
        var bonusArray = new ItemBonus[7];
        for (var i = 0; i < 7; i++)
        {
            bonusArray[i] = new ItemBonus
            {
                BonusId = 0,
                Value = 0
            };
        }
        
        var test = new MallItem
        {
            Cell = new MallItemPosition
            {
                Type = 4,
                Cell = 1
            },
            Vid = proto.Id, // this is wrong, but it's here for testing purposes only
            Count = 1,
            Flags = proto.Flags,
            AntiFlags = proto.AntiFlags,
            Highlight = 0,
            Sockets = new uint[3],
            Bonuses = bonusArray
        };
        
        _connection.Send(test);
    }

    public void Close()
    {
        // todo save items to db
        LastInteraction = DateTime.UtcNow;
        SendChatCommand("CloseMall");
    }
    
    private void SendChatCommand(string message)
    {
        var chat = new ChatOutcoming
        {
            MessageType = ChatMessageTypes.Command,
            Message = message
        };
        _connection.Send(chat);
    }
}
