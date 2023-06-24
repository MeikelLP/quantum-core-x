using System;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Logging;
using QuantumCore.API.Game.World;
using QuantumCore.Database;
using QuantumCore.Game.PacketHandlers.Game;
using QuantumCore.Game.Packets.Affects;
using Dapper;
using System.Linq;
using Affect = QuantumCore.Database.Affect;
using AffectAPI = QuantumCore.API.Core.Models.Affect;
using System.Numerics;

namespace QuantumCore.Game
{
    public class AffectController : IAffectController
    {
        private readonly ILogger<ItemUseHandler> _logger;
        private readonly IDatabaseManager _databaseManager;

        public AffectController(ILogger<ItemUseHandler> logger, IDatabaseManager databaseManager) {
            _logger = logger;
            _databaseManager = databaseManager;
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
            var db = _databaseManager.GetGameDatabase();
            await db.QueryAsync("DELETE FROM affects WHERE PlayerId=@PlayerId and Type=@Type and ApplyOn=@ApplyOn", 
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
            var db = _databaseManager.GetGameDatabase();
            //var playerAffects = await db.QueryAsync<Affect>("SELECT * FROM affects WHERE PlayerId = @PlayerId", new { PlayerId = playerEntity.Player.Id });
            
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
                    await _databaseManager.GetGameDatabase().InsertAsync(affect);
                    await playerEntity.AddAffect(affectAPI);
                    await playerEntity.SendChatInfo("This affect duration is extended!");
                }
            }
            else
            {
                await _databaseManager.GetGameDatabase().InsertAsync(affect);
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
            var db = _databaseManager.GetGameDatabase();
            var playerAffects = await db.QueryAsync<Affect>("SELECT * FROM affects WHERE PlayerId = @PlayerId", new { PlayerId = playerEntity.Player.Id });


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
