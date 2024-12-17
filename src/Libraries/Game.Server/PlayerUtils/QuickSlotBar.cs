using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Caching;
using QuantumCore.Game.Packets.General;
using QuantumCore.Game.Packets.QuickBar;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.Persistence.Entities;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.PlayerUtils;

public class QuickSlotBar : IQuickSlotBar
{
    private readonly IRedisStore _cacheManager;
    private readonly ILogger _logger;
    private readonly GameDbContext _db;
    public IPlayerEntity Player { get; }
    public QuickSlotData?[] Slots { get; } = new QuickSlotData[8];

    public QuickSlotBar(ICacheManager cacheManager, ILogger<QuickSlotBar> logger, PlayerEntity player, GameDbContext db)
    {
        _cacheManager = cacheManager.Server;
        _logger = logger;
        _db = db;
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

            return;
        }

        var dbSlots = await _db.PlayerQuickSlots
            .AsNoTracking()
            .Where(x => x.PlayerId == Player.Player.Id)
            .ToDictionaryAsync(x => x.Slot);

        for (var i = 0; i < Slots.Length; i++)
        {
            Slots[i] = dbSlots.TryGetValue((byte)i, out var dbSlot)
                ? new QuickSlotData {Type = dbSlot.Type, Position = dbSlot.Value}
                : null;
        }

        await _cacheManager.Set(key, Slots);
    }

    public async Task Persist()
    {
        var key = $"player:quickbar:{Player.Player.Id}";

        await _cacheManager.Set(key, Slots);
        var dbPlayer = await _db.Players
            .Include(x => x.QuickSlots)
            .FirstAsync(x => x.Id == Player.Player.Id);
        dbPlayer.QuickSlots.Clear();
        for (var i = 0; i < Slots.Length; i++)
        {
            var slot = Slots[i];
            if (slot is null) continue;
            dbPlayer.QuickSlots.Add(new PlayerQuickSlot {Slot = (byte)i, Type = slot.Type, Value = slot.Position});
        }

        await _db.SaveChangesAsync();
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
                Position = (byte)i, Slot = new QuickSlot {Position = slot.Position, Type = slot.Type}
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
            Position = position, Slot = new QuickSlot {Position = slot.Position, Type = slot.Type}
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
        Player.Connection.Send(new QuickBarSwapOut {Position1 = position1, Position2 = position2});
    }

    public void Remove(byte position)
    {
        if (position >= 8)
        {
            return;
        }

        Slots[position] = null;
        Player.Connection.Send(new QuickBarRemoveOut {Position = position});
    }
}
