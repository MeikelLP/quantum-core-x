using System;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Logging;
using QuantumCore.API.Game.World;
using QuantumCore.Database;
using QuantumCore.Game.PacketHandlers.Game;
using QuantumCore.Game.Packets.Affects;
using QuantumCore.API.Core.Models;
using Dapper;
using System.Linq;

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

        public void SendAffectRemovePacket(IPlayerEntity playerEntity, Affect affect)
        {
            throw new NotImplementedException();
        }

        public async Task AddAffect(IPlayerEntity playerEntity, int type, int applyOn, int applyValue, int flag, int duration, int spCost)
        {
            _logger.LogDebug("Add affect starting!");
            _logger.LogDebug("::AddAffect Type:{Type}, ApplyOn:{ApplyOn}, ApplyValue:{ApplyValue}, Flag:{Flag}, Duration:{Duration}, SpCost:{SpCost}", type, applyOn, applyValue, flag, duration, spCost);  
            var db = _databaseManager.GetGameDatabase();
            var playerAffects = await db.QueryAsync<Affect>("SELECT * FROM affects WHERE PlayerId = @PlayerId", new { PlayerId = playerEntity.Player.Id });
            
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

            if (playerAffects != null && playerAffects.Any())
            {
                var sameThing = false;
                foreach(var playerAffect in playerAffects){
                    if(playerAffect.Type == type) // must be uniq
                    {
                        sameThing = true;
                        if (playerAffect.ApplyValue == applyValue &&  playerAffect.ApplyOn == applyOn)
                        {
                            // update duration of the affect 
                            playerAffect.Duration = playerAffect.Duration.AddSeconds(duration);
                            await _databaseManager.GetGameDatabase().UpdateAsync(playerAffect);
                        }
                        else
                        {
                            await playerEntity.SendChatInfo("This affect is already working!");
                        }
                    }
                    break;
                }
                if(!sameThing)
                {
                    // create new affect
                    await _databaseManager.GetGameDatabase().InsertAsync(affect);
                }
            } 
            else 
            {
                // create new affect
                await _databaseManager.GetGameDatabase().InsertAsync(affect);
            }

            // Add affect to cache
            // await _cacheManager.Set("affect:" + player.Id, player);

            SendAffectAddPacket(playerEntity, affect, duration);

        }

        bool IAffectController.RemoveAffect()
        {
            throw new NotImplementedException();
        }
    }
}
