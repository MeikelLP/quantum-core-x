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
    private readonly IRedisStore _cacheManager;
    private readonly ILogger _logger;
    public IPlayerEntity Player { get; }
    public QuickSlotData?[] Slots { get; } = new QuickSlotData[8];

    public QuickSlotBar(ICacheManager cacheManager, ILogger logger, PlayerEntity player)
    {
        _cacheManager = cacheManager.Server;
        _logger = logger;
        Player = player;
    }

    public async Task Load()
    {
        var key = $"player:quickbar:{Player.Player.Id}";

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
        var key = $"player:quickbar:{Player.Player.Id}";

        await _cacheManager.Set(key, Slots);
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

            Player.Connection.Send(new QuickBarAddOut
            {
                Position = (byte) i,
                Slot = new QuickSlot {Position = slot.Position, Type = slot.Type}
            });
        }
    }

    public void Add(byte position, QuickSlotData slot)
    {
        if (position >= 8)
        {
            return;
        }

        // todo verify type, and position?

        Slots[position] = slot;
        Player.Connection.Send(new QuickBarAddOut
        {
            Position = position,
            Slot = new QuickSlot {Position = slot.Position, Type = slot.Type}
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
        Player.Connection.Send(new QuickBarSwapOut
        {
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
        Player.Connection.Send(new QuickBarRemoveOut
        {
            Position = position
        });
    }
}