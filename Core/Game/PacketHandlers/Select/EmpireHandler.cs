using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Database;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Select;

public class EmpireHandler : IGamePacketHandler<Empire>
{
    private readonly ILogger<EmpireHandler> _logger;
    private readonly IDatabaseManager _databaseManager;

    public EmpireHandler(ILogger<EmpireHandler> logger, IDatabaseManager databaseManager)
    {
        _logger = logger;
        _databaseManager = databaseManager;
    }

    public async Task ExecuteAsync(GamePacketContext<Empire> ctx, CancellationToken token = default)
    {
        if (ctx.Packet.EmpireId is > 0 and < 4)
        {
            using var db = _databaseManager.GetAccountDatabase();
            var result = await db.ExecuteAsync("UPDATE accounts set Empire = @Empire WHERE Id = @AccountId"
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