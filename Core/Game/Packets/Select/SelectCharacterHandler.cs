using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.API.Game.Types;
using QuantumCore.Core.Networking;
using QuantumCore.Database;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Packets.Select;

public class SelectCharacterHandler : ISelectPacketHandler<SelectCharacter>
{
    private readonly ILogger<SelectCharacterHandler> _logger;
    private readonly IServiceProvider _provider;
    private readonly IDatabaseManager _databaseManager;

    public SelectCharacterHandler(ILogger<SelectCharacterHandler> logger, IServiceProvider provider, IDatabaseManager databaseManager)
    {
        _logger = logger;
        _provider = provider;
        _databaseManager = databaseManager;
    }
    
    public async Task ExecuteAsync(PacketContext<SelectCharacter> ctx, CancellationToken token = default)
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
        await ctx.Connection.SetPhase(EPhases.Loading);

        // Load player
        var player = await Player.GetPlayer(_databaseManager, accountId, ctx.Packet.Slot);
        var entity = ActivatorUtilities.CreateInstance<PlayerEntity>(_provider, ctx.Connection, player);
        await entity.Load();

        ctx.Connection.Player = entity;

        // Send information about the player to the client
        await entity.SendBasicData();
        await entity.SendPoints();
        await entity.QuickSlotBar.Send();
    }
}