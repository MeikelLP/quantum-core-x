using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.API.PluginTypes;
using QuantumCore.Extensions;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Select;

public class SelectGameCharacterHandler : IGamePacketHandler<SelectCharacter>
{
    private readonly ILogger<SelectGameCharacterHandler> _logger;
    private readonly IPlayerManager _playerManager;
    private readonly IPlayerFactory _playerFactory;

    public SelectGameCharacterHandler(ILogger<SelectGameCharacterHandler> logger,
        IPlayerManager playerManager, IPlayerFactory playerFactory)
    {
        _logger = logger;
        _playerManager = playerManager;
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

        var accountId = ctx.Connection.AccountId.Value;

        // Let the client load the game
        ctx.Connection.SetPhase(EPhases.Loading);

        // Load player
        var player = await _playerManager.GetPlayer(accountId, ctx.Packet.Slot);
        if (player is null)
        {
            throw new InvalidOperationException("Player was not found. This should never happen at this point");
        }

        var entity = await _playerFactory.CreatePlayerAsync(ctx.Connection, player);

        ctx.Connection.Player = entity;

        // Send information about the player to the client
        entity.SendBasicData();
        entity.SendPoints();
        entity.QuickSlotBar.Send();
    }
}