using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Database;
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
            var empire = new Empires {
                Id = Guid.NewGuid(),
                AccountId = (Guid)ctx.Connection.AccountId,
                Empire = ctx.Packet.EmpireId 
            };  
            var result = await _db.InsertAsync(empire);
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