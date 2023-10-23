using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Logging;
using QuantumCore.API.Game.World;
using Dapper;
using System.Data;
using QuantumCore.API.Game.Types;
using Affect = QuantumCore.Database.Affect;
using AffectAPI = QuantumCore.API.Core.Models.Affect;
using AffectRemove = QuantumCore.Game.Packets.AffectRemove;

namespace QuantumCore.Game;

public class AffectManager : IAffectManager
{
    private readonly ILogger<IAffectManager> _logger;
    private readonly IDbConnection _db;

    public AffectManager(ILogger<IAffectManager> logger, IDbConnection db) {
        _logger = logger;
        _db = db;
    }

    public async Task SendAffectRemovePacket(IPlayerEntity playerEntity, EAffectType type, EAffectType applyOn)
    {
        await _db.QueryAsync("DELETE FROM affects WHERE PlayerId=@PlayerId and Type=@Type and ApplyOn=@ApplyOn",
            new { PlayerId = playerEntity.Player.Id, Type = type, ApplyOn = applyOn });
        var affectRemovePacket = new AffectRemove
        {
            Type = (uint) type,
            ApplyOn = applyOn,
        };
        await playerEntity.Connection.Send(affectRemovePacket);
    }

    public async Task AddAffect(IPlayerEntity playerEntity, EAffectType type, EAffectType applyOn, int applyValue,
        EAffects flags,
        int duration, int spCost)
    {

        // Create player data
        var affect = new Affect
        {
            PlayerId = playerEntity.Player.Id,
            Type = type,
            ApplyOn = applyOn,
            ApplyValue = applyValue,
            Flag = flags,
            Duration = DateTime.Now.AddSeconds(duration),
            SpCost = spCost
        };
        var affectAPI = new AffectAPI
        {
            PlayerId = playerEntity.Player.Id,
            Type = type,
            ApplyOn = applyOn,
            ApplyValue = applyValue,
            Flag = flags,
            Duration = DateTime.Now.AddSeconds(duration),
            SpCost = spCost
        };
        _logger.LogDebug("Adding affect to player {PlayerName}: {@Affect}", playerEntity.Name, affect);

        if (playerEntity.TryGetAffect(affectAPI, out var affectApi))
        {
            if(affect.ApplyValue != affectAPI.ApplyValue)
            {
                await playerEntity.SendChatInfo("This affect is already working!");
            }
            else
            {
                await playerEntity.RemoveAffect(affectApi);
                affectApi.Duration = affectApi.Duration.AddSeconds(duration);
                affect.Duration = affectApi.Duration;
                await _db.InsertAsync(affect);
                await playerEntity.AddAffect(affectAPI);
                await playerEntity.SendChatInfo("This affect duration is extended!");
            }
        }
        else
        {
            await _db.InsertAsync(affect);
            await playerEntity.AddAffect(affectAPI);
        }

        // Add affect to cache
        // await _cacheManager.Set("affect:" + player.Id, player);
    }

    public async Task LoadAffect(IPlayerEntity playerEntity)
    {
        var playerAffects = await _db.QueryAsync<Affect>("SELECT * FROM affects WHERE PlayerId = @PlayerId", new { PlayerId = playerEntity.Player.Id });
        if (playerAffects != null && playerAffects.Any())
        {
            foreach(var playerAffect in playerAffects)
            {
                var affect = new AffectAPI
                {
                    PlayerId = playerAffect.PlayerId,
                    Type = playerAffect.Type,
                    ApplyOn = playerAffect.ApplyOn,
                    ApplyValue = playerAffect.ApplyValue,
                    Flag = playerAffect.Flag,
                    Duration = playerAffect.Duration,
                    SpCost = playerAffect.SpCost
                };
                await playerEntity.AddAffect(affect);
            }
        }
        _logger.LogDebug("Loaded affects for player: {PlayerName}", playerEntity.Name);
    }
}
