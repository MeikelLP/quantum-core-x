using System.Threading.Tasks;
using QuantumCore.Cache;
using QuantumCore.Game.Packets;
using QuantumCore.Game.Packets.General;
using QuantumCore.Game.Packets.QuickBar;
using QuantumCore.Game.World.Entities;
using Serilog;

namespace QuantumCore.Game.PlayerUtils;

public class QuickSlotBar
{
    public PlayerEntity Player { get; }
    public QuickSlot[] Slots { get; } = new QuickSlot[8];
    
    public QuickSlotBar(PlayerEntity player)
    {
        Player = player;
    }

    public async Task Load()
    {
        var redis = CacheManager.Redis;
        var key = $"quickbar:{Player.Player.Id}";
        
        if (await redis.Exists(key) > 0)
        {
            var slots = await redis.Get<QuickSlot[]>(key);
            if (slots.Length != Slots.Length)
            {
                Log.Warning("Removing cached quick slots, length mismatch");
                await redis.Del(key);
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
        var redis = CacheManager.Redis;
        var key = $"quickbar:{Player.Player.Id}";

        await redis.Set(key, Slots);
    }

    public void Send()
    {
        for (var i = 0; i < Slots.Length; i++)
        {
            var slot = Slots[i];
            if (slot == null)
            {
                continue;
            }
            
            Player.Connection.Send(new QuickBarAddOut {
                Position = (byte) i,
                Slot = slot
            });
        }
    }

    public void Add(byte position, QuickSlot slot)
    {
        if (position >= 8)
        {
            return;
        }

        // todo verify type, and position?
        
        Slots[position] = slot;
        Player.Connection.Send(new QuickBarAddOut {
            Position = position,
            Slot = slot
        });
    }

    public void Swap(byte position1, byte position2)
    {
        if (position1 >= 8 || position2 >= 8)
        {
            return;
        }

        var slot1 = Slots[position1];
        var slot2 = Slots[position2];
        Slots[position1] = slot2;
        Slots[position2] = slot1;
        Player.Connection.Send(new QuickBarSwapOut {
            Position1 = position1,
            Position2 = position2
        });
    }

    public void Remove(byte position)
    {
        if (position >= 8)
        {
            return;
        }

        Slots[position] = null;
        Player.Connection.Send(new QuickBarRemoveOut {
            Position = position
        });
    }
}