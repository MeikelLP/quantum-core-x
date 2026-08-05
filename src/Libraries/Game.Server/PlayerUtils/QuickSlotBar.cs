using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.API.Packets.QuickBar;
using QuantumCore.Game.Packets.General;
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
    public ImmutableArray<QuickSlotData?> Slots => [.. _slots];
    private readonly QuickSlotData?[] _slots = new QuickSlotData?[8];

    public QuickSlotBar(ICacheManager cacheManager, ILogger<QuickSlotBar> logger, PlayerEntity player, GameDbContext db)
    {
        ArgumentNullException.ThrowIfNull(cacheManager);
        _cacheManager = cacheManager.Server;
        _logger = logger;
        _db = db;
        Player = player;
    }

    public async Task LoadAsync()
    {
        var key = $"player:quickbar:{Player.Player.Id}";

        if (await _cacheManager.ExistsAsync(key) > 0)
        {
            var slots = await _cacheManager.GetAsync<QuickSlotData[]>(key);
            if (slots.Length != _slots.Length)
            {
                _logger.LogWarning("Removing cached quick slots, length mismatch");
                await _cacheManager.DelAsync(key);
            }
            else
            {
                for (var i = 0; i < slots.Length; i++)
                {
                    _slots[i] = slots[i];
                }
            }

            return;
        }

        var dbSlots = await _db.PlayerQuickSlots
            .AsNoTracking()
            .Where(x => x.PlayerId == Player.Player.Id)
            .ToDictionaryAsync(x => x.Slot);

        for (var i = 0; i < _slots.Length; i++)
        {
            _slots[i] = dbSlots.TryGetValue((byte)i, out var dbSlot)
                ? new QuickSlotData { Type = dbSlot.Type, Position = dbSlot.Value }
                : null;
        }

        await _cacheManager.SetAsync(key, _slots);
    }

    public async Task PersistAsync()
    {
        var key = $"player:quickbar:{Player.Player.Id}";

        await _cacheManager.SetAsync(key, _slots);
        var dbPlayer = await _db.Players
            .Include(x => x.QuickSlots)
            .FirstAsync(x => x.Id == Player.Player.Id);
        dbPlayer.QuickSlots.Clear();
        for (var i = 0; i < _slots.Length; i++)
        {
            var slot = _slots[i];
            if (slot is null) continue;
            dbPlayer.QuickSlots.Add(new PlayerQuickSlot { Slot = (byte)i, Type = slot.Type, Value = slot.Position });
        }

        await _db.SaveChangesAsync();
    }

    public void Send()
    {
        for (var i = 0; i < _slots.Length; i++)
        {
            var slot = _slots[i];
            if (slot is null)
            {
                continue;
            }

            Player.Connection.Send(new QuickBarAddOut
            {
                Position = (byte)i, Slot = new QuickSlot { Position = slot.Position, Type = slot.Type }
            });
        }
    }

    public void Add(byte position, QuickSlotData slot)
    {
        ArgumentNullException.ThrowIfNull(slot);
        if (position >= 8)
        {
            return;
        }

        // todo verify type, and position?

        _slots[position] = slot;
        Player.Connection.Send(new QuickBarAddOut
        {
            Position = position, Slot = new QuickSlot { Position = slot.Position, Type = slot.Type }
        });
    }

    public void Swap(byte position1, byte position2)
    {
        if (position1 >= 8 || position2 >= 8)
        {
            return;
        }

        var slot1 = _slots[position1];
        var slot2 = _slots[position2];
        _slots[position1] = slot2;
        _slots[position2] = slot1;
        Player.Connection.Send(new QuickBarSwapOut { Position1 = position1, Position2 = position2 });
    }

    public void Remove(byte position)
    {
        if (position >= 8)
        {
            return;
        }

        _slots[position] = null;
        Player.Connection.Send(new QuickBarRemoveOut { Position = position });
    }
}