using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.API.PluginTypes;
using QuantumCore.Core.Cache;
using QuantumCore.Database;
using QuantumCore.Database.Repositories;
using QuantumCore.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.PacketHandlers.Select;

public class SelectGameCharacterHandler : IGamePacketHandler<SelectCharacter>
{
    private readonly ILogger<SelectGameCharacterHandler> _logger;
    private readonly IServiceProvider _provider;
    private readonly ICacheManager _cacheManager;
    private readonly IPlayerRepository _playerRepository;

    public SelectGameCharacterHandler(ILogger<SelectGameCharacterHandler> logger, IServiceProvider provider,
        ICacheManager cacheManager, IPlayerRepository playerRepository)
    {
        _logger = logger;
        _provider = provider;
        _cacheManager = cacheManager;
        _playerRepository = playerRepository;
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
        ctx.Connection.SetPhase(EPhases.Loading);

        // Load player
        var player = await Player.GetPlayer(_playerRepository, _cacheManager, accountId, ctx.Packet.Slot);
        var entity = ActivatorUtilities.CreateInstance<PlayerEntity>(_provider, ctx.Connection, player);
        await entity.Load();

        ctx.Connection.Player = entity;

        // Send information about the player to the client
        entity.SendBasicData();
        entity.SendPoints();
        entity.SendCharacterUpdate();
        entity.QuickSlotBar.Send();
    }
}
