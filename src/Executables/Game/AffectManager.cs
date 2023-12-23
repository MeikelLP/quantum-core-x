using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game;

public class AffectManager : IAffectManager
{
    private readonly ILogger<IAffectManager> _logger;
    private readonly IAffectRepository _repository;

    public AffectManager(ILogger<AffectManager> logger, IAffectRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task RemoveAffectFromPlayerAsync(IPlayerEntity playerEntity, EAffectType type, EApplyType applyOn)
    {
        await _repository.RemoveAffectFromPlayerAsync(playerEntity.Player.Id, type, applyOn);
        var affectRemovePacket = new AffectRemove
        {
            Type = (uint) type,
            ApplyOn = applyOn,
        };
        playerEntity.Connection.Send(affectRemovePacket);
    }

    public async Task AddAffectToPlayerAsync(IPlayerEntity playerEntity, EAffectType type, EApplyType applyOn, int applyValue,
        EAffects flags, int duration, int spCost)
    {
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
        _logger.LogDebug("Adding affect to player {PlayerName}: {@Affect}", playerEntity.Name, affect);

        if (playerEntity.TryGetAffect(affect, out var existingAffect))
        {
            if(existingAffect.ApplyValue != affect.ApplyValue)
            {
                playerEntity.SendChatInfo("This affect is already working!");
            }
            else
            {
                playerEntity.RemoveAffect(existingAffect);
                existingAffect.Duration = existingAffect.Duration.AddSeconds(duration);
                await _repository.AddAffectAsync(affect);
                playerEntity.AddAffect(affect);
                playerEntity.SendChatInfo("This affect duration is extended!");
            }
        }
        else
        {
            await _repository.AddAffectAsync(affect);
            playerEntity.AddAffect(affect);
        }

        // TODO: Add affect to cache
        // await _cacheManager.Set("affect:" + player.Id, player);
    }

    public async Task LoadAffectAffectsForPlayer(IPlayerEntity playerEntity)
    {
        var playerAffects = await _repository.GetAffectsForPlayerAsync(playerEntity.Player.Id);
        foreach(var playerAffect in playerAffects)
        {
            var affect = new Affect
            {
                PlayerId = playerAffect.PlayerId,
                Type = playerAffect.Type,
                ApplyOn = playerAffect.ApplyOn,
                ApplyValue = playerAffect.ApplyValue,
                Flag = playerAffect.Flag,
                Duration = playerAffect.Duration,
                SpCost = playerAffect.SpCost
            };
            playerEntity.AddAffect(affect);
        }
        _logger.LogDebug("Loaded affects for player: {PlayerName}", playerEntity.Name);
    }
}
