using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Select;

public class EmpireHandler : IGamePacketHandler<Empire>
{
    private readonly ILogger<EmpireHandler> _logger;
    private readonly IDbConnection _db;

    public EmpireHandler(ILogger<EmpireHandler> logger, IDbConnection db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task ExecuteAsync(GamePacketContext<Empire> ctx, CancellationToken token = default)
    {
        if (ctx.Packet.EmpireId is > 0 and < 4)
        {
            var result = await _db.ExecuteAsync("UPDATE account.accounts set Empire = @Empire WHERE Id = @AccountId"
                , new { AccountId = ctx.Connection.AccountId, Empire = ctx.Packet.EmpireId });
            if (result is not 1)
            {
                _logger.LogWarning("Unexpected result count {Result} when setting empire for account", result);
            }
        }
        else
        {
            _logger.LogWarning("Unexpected empire choice {Empire}", ctx.Packet.EmpireId);
        }
    }
}