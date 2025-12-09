using EnumsNET;
using Game.Caching;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.API.PluginTypes;
using QuantumCore.Caching;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Select;

public class EmpireHandler : IGamePacketHandler<Empire>
{
    private readonly ILogger<EmpireHandler> _logger;
    private readonly IPlayerManager _playerManager;
    private readonly IRedisStore _cacheManager;
    private readonly ICachePlayerRepository _playerCache;

    public EmpireHandler(ILogger<EmpireHandler> logger, IPlayerManager playerManager, ICacheManager cacheManager,
        ICachePlayerRepository playerCache)
    {
        _logger = logger;
        _playerManager = playerManager;
        _cacheManager = cacheManager.Server;
        _playerCache = playerCache;
    }

    public async Task ExecuteAsync(GamePacketContext<Empire> ctx, CancellationToken token = default)
    {
        if (Enums.TryToObject<EEmpire>(ctx.Packet.EmpireId, out var empire, EnumValidation.IsDefined))
        {
            _logger.LogInformation("Empire selected: {Empire}", empire);
            var cacheKey = $"account:{ctx.Connection.AccountId}:game:select:selected-player";
            var player = await _cacheManager.Get<uint?>(cacheKey);
            if (player is not null)
            {
                await _playerManager.SetPlayerEmpireAsync(ctx.Connection.AccountId!.Value, player.Value, empire);
                await _cacheManager.Del(cacheKey);
            }
            else
            {
                // No player created yet. This is the first time an empire is selected before creating an account
                await _playerCache.SetTempEmpireAsync(ctx.Connection.AccountId!.Value, empire);
            }
        }
        else
        {
            _logger.LogWarning("Unexpected empire choice {Empire}", ctx.Packet.EmpireId);
        }
    }
}
