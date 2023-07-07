using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Logging;
using QuantumCore.API.Game.World;
using Dapper;
using System.Data;
using Affect = QuantumCore.Database.Affect;
using AffectAPI = QuantumCore.API.Core.Models.Affect;
using QuantumCore.Game.Packets.Affects;

namespace QuantumCore.Game
{
    public class AffectController : IAffectController
    {
        private readonly ILogger<IAffectController> _logger;
        private readonly IDbConnection _db;

        public AffectController(ILogger<IAffectController> logger, IDbConnection db) {
            _logger = logger;
            _db = db;
        }
        public void SendAffectAddPacket(IPlayerEntity playerEntity, Affect affect, int duration)
        {
            var affectAdd = new AffectAdd();
            var affectAddPacket = new AffectAddPacket
            {
                Type = (uint) affect.Type,
                ApplyOn = affect.ApplyOn,
                ApplyValue = (uint) affect.ApplyValue,
                Duration = (uint) duration,
                Flag = (uint) affect.Flag,
                SpCost = (uint) affect.SpCost
            };
            affectAdd.Elem[0] = affectAddPacket;
            playerEntity.Connection.Send(affectAdd);
        }

        public async Task SendAffectRemovePacket(IPlayerEntity playerEntity, long type, byte applyOn)
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

        public async Task AddAffect(IPlayerEntity playerEntity, int type, int applyOn, int applyValue, int flag, int duration, int spCost)
        {
            _logger.LogDebug("Add affect starting!");
            _logger.LogDebug("::AddAffect Type:{Type}, ApplyOn:{ApplyOn}, ApplyValue:{ApplyValue}, Flag:{Flag}, Duration:{Duration}, SpCost:{SpCost}", type, applyOn, applyValue, flag, duration, spCost);  
            
            // Create player data
            var affect = new Affect
            {
                PlayerId = playerEntity.Player.Id,
                Type = type,
                ApplyOn = (byte) applyOn,
                ApplyValue = applyValue,
                Flag = flag,
                Duration = DateTime.Now.AddSeconds(duration),
                SpCost = spCost
            };
            var affectAPI = new AffectAPI
            {
                PlayerId = playerEntity.Player.Id,
                Type = type,
                ApplyOn = (byte) applyOn,
                ApplyValue = applyValue,
                Flag = flag,
                Duration = DateTime.Now.AddSeconds(duration),
                SpCost = spCost
            };

            var affectApi = playerEntity.HasAffect(affectAPI);
            if (affectApi != null)
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

            SendAffectAddPacket(playerEntity, affect, duration);

        }

        public async Task LoadAffect(IPlayerEntity playerEntity)
        {
            _logger.LogDebug("Load affect starting!");
            _logger.LogDebug("::LoadAffect PlayerId:{}", playerEntity.Player.Id);
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


        }
    }
}
