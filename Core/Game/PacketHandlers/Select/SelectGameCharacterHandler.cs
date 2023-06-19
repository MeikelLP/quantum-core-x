using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.API.PluginTypes;
using QuantumCore.Core.Cache;
using QuantumCore.Database;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.PacketHandlers.Select;

public class SelectGameCharacterHandler : IGamePacketHandler<SelectCharacter>
{
    private readonly IDbConnection _db;
    private readonly ILogger<SelectGameCharacterHandler> _logger;
    private readonly IServiceProvider _provider;
    private readonly ICacheManager _cacheManager;

    public SelectGameCharacterHandler(IDbConnection db, ILogger<SelectGameCharacterHandler> logger, IServiceProvider provider, 
        ICacheManager cacheManager)
    {
        _db = db;
        _logger = logger;
        _provider = provider;
        _cacheManager = cacheManager;
    }
    
    public async Task ExecuteAsync(GamePacketContext<SelectCharacter> ctx, CancellationToken token = default)
    {
        _logger.LogDebug("Selected character in slot {Slot}", ctx.Packet.Slot);
        if (ctx.Connection.AccountId == null)
        {
            // We didn't received any login before
            ctx.Connection.Close();
            _logger.LogWarning("Character select received before authorization");
            return;
        }

        var accountId = ctx.Connection.AccountId ?? default; // todo clean solution

        // Let the client load the game
        await ctx.Connection.SetPhaseAsync(EPhases.Loading);

        // Load player
        var player = await Player.GetPlayer(_db, _cacheManager, accountId, ctx.Packet.Slot);
        var entity = ActivatorUtilities.CreateInstance<PlayerEntity>(_provider, ctx.Connection, player);
        await entity.Load();

        ctx.Connection.Player = entity;

        // Send information about the player to the client
        await entity.SendBasicData();
        await entity.SendPoints();
        await entity.SendCharacterUpdate();
        await entity.QuickSlotBar.Send();
    }
}