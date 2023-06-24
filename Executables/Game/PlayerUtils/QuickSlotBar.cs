using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Caching;
using QuantumCore.Game.Packets.General;
using QuantumCore.Game.Packets.QuickBar;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.PlayerUtils;

public class QuickSlotBar : IQuickSlotBar
{
    private readonly ICacheManager _cacheManager;
    private readonly ILogger _logger;
    public IPlayerEntity Player { get; }
    public QuickSlotData[] Slots { get; } = new QuickSlotData[8];
    
    public QuickSlotBar(ICacheManager cacheManager, ILogger logger, PlayerEntity player)
    {
        _cacheManager = cacheManager;
        _logger = logger;
        Player = player;
    }

    public async Task Load()
    {
        var key = $"quickbar:{Player.Player.Id}";
        
        if (await _cacheManager.Exists(key) > 0)
        {
            var slots = await _cacheManager.Get<QuickSlotData[]>(key);
            if (slots.Length != Slots.Length)
            {
                _logger.LogWarning("Removing cached quick slots, length mismatch");
                await _cacheManager.Del(key);
            }
            else
            {
                for (var i = 0; i < slots.Length; i++)
                {
                    Slots[i] = slots[i];
                }
            }
        }
        
        // todo load from database
    }

    public async Task Persist()
    {
        var key = $"quickbar:{Player.Player.Id}";

        await _cacheManager.Set(key, Slots);
    }

    public async Task Send()
    {
        for (var i = 0; i < Slots.Length; i++)
        {
            var slot = Slots[i];
            if (slot == null)
            {
                continue;
            }
            
            await Player.Connection.Send(new QuickBarAddOut {
                Position = (byte) i,
                Slot = new QuickSlot{Position = slot.Position, Type = slot.Type}
            });
        }
    }

    public async Task Add(byte position, QuickSlotData slot)
    {
        if (position >= 8)
        {
            return;
        }

        // todo verify type, and position?
        
        Slots[position] = slot;
        await Player.Connection.Send(new QuickBarAddOut {
            Position = position,
            Slot = new QuickSlot{Position = slot.Position, Type = slot.Type}
        });
    }

    public async Task Swap(byte position1, byte position2)
    {
        if (position1 >= 8 || position2 >= 8)
        {
            return;
        }

        var slot1 = Slots[position1];
        var slot2 = Slots[position2];
        Slots[position1] = slot2;
        Slots[position2] = slot1;
        await Player.Connection.Send(new QuickBarSwapOut {
            Position1 = position1,
            Position2 = position2
        });
    }

    public async Task Remove(byte position)
    {
        if (position >= 8)
        {
            return;
        }

        Slots[position] = null;
        await Player.Connection.Send(new QuickBarRemoveOut {
            Position = position
        });
    }
}