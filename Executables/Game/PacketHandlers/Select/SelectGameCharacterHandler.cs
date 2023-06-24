using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.PacketHandlers.Select;

public class SelectGameCharacterHandler : IGamePacketHandler<SelectCharacter>
{
    private readonly ILogger<SelectGameCharacterHandler> _logger;
    private readonly IServiceProvider _provider;
    private readonly IPlayerFactory _playerFactory;

    public SelectGameCharacterHandler(ILogger<SelectGameCharacterHandler> logger, IServiceProvider provider, 
        IPlayerFactory playerFactory)
    {
        _logger = logger;
        _provider = provider;
        _playerFactory = playerFactory;
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
        var player = await _playerFactory.GetPlayer(accountId, ctx.Packet.Slot);
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