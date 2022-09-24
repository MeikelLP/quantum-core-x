using System.Threading.Tasks;
using QuantumCore.Core.Cache;
using QuantumCore.Game.Packets;
using QuantumCore.Game.Packets.General;
using QuantumCore.Game.Packets.QuickBar;
using QuantumCore.Game.World.Entities;
using Serilog;

namespace QuantumCore.Game.PlayerUtils;

public class QuickSlotBar
{
    private readonly ICacheManager _cacheManager;
    public PlayerEntity Player { get; }
    public QuickSlot[] Slots { get; } = new QuickSlot[8];
    
    public QuickSlotBar(ICacheManager cacheManager, PlayerEntity player)
    {
        _cacheManager = cacheManager;
        Player = player;
    }

    public async Task Load()
    {
        var key = $"quickbar:{Player.Player.Id}";
        
        if (await _cacheManager.Exists(key) > 0)
        {
            var slots = await _cacheManager.Get<QuickSlot[]>(key);
            if (slots.Length != Slots.Length)
            {
                Log.Warning("Removing cached quick slots, length mismatch");
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
                Slot = slot
            });
        }
    }

    public async Task Add(byte position, QuickSlot slot)
    {
        if (position >= 8)
        {
            return;
        }

        // todo verify type, and position?
        
        Slots[position] = slot;
        await Player.Connection.Send(new QuickBarAddOut {
            Position = position,
            Slot = slot
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