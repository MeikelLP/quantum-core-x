using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using QuantumCore.API.Game.Types;
using QuantumCore.Cache;
using QuantumCore.Core.Constants;
using QuantumCore.Core.Networking;
using QuantumCore.Core.Utils;
using QuantumCore.Database;
using QuantumCore.Game.Packets;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.World;
using QuantumCore.Game.World.Entities;
using Serilog;

namespace QuantumCore.Game
{
    public static class PhaseSelect
    {
        [Listener(typeof(SelectCharacter))]
        public static async Task OnSelectCharacter(this GameConnection connection, SelectCharacter packet)
        {
            Log.Debug($"Selected character in slot {packet.Slot}");
            if (connection.AccountId == null)
            {
                // We didn't received any login before
                connection.Close();
                Log.Warning($"Character select received before authorization");
                return;
            }

            var accountId = connection.AccountId ?? default; // todo clean solution
            
            // Let the client load the game
            await connection.SetPhase(EPhases.Loading);
            
            // Load player
            var player = await Player.GetPlayer(accountId, packet.Slot);
            var entity = new PlayerEntity(player, connection);
            await entity.Load();
            
            connection.Player = entity;

            // Send information about the player to the client
            await entity.SendBasicData();
            await entity.SendPoints();
            await entity.QuickSlotBar.Send();
        }

        [Listener(typeof(DeleteCharacter))]
        public static async Task OnDeleteCharacter(this GameConnection connection, DeleteCharacter packet)
        {
            Log.Debug($"Deleting character in slot {packet.Slot}");

            if (connection.AccountId == null)
            {
                connection.Close();
                Log.Warning("Character remove received before authorization");
                return;
            }

            var accountId = connection.AccountId ?? default;

            var db = DatabaseManager.GetAccountDatabase();
            var deletecode = await db.QueryFirstOrDefaultAsync<string>("SELECT DeleteCode FROM accounts WHERE Id = @Id", new { Id = connection.AccountId });

            if (deletecode == default)
            {
                connection.Close();
                Log.Warning("Invalida ccount id??");
                return;
            }

            if (deletecode != packet.Code[..^1])
            {
                await connection.Send(new DeleteCharacterFail());
                return;
            }

            await connection.Send(new DeleteCharacterSuccess
            {
                Slot = packet.Slot
            });

            var player = await Player.GetPlayer(accountId, packet.Slot);
            if (player == null)
            {
                connection.Close();
                Log.Warning("Invalid or not exist character");
                return;
            }

            db = DatabaseManager.GetGameDatabase();

            var delPlayer = new PlayerDeleted(player);
            await db.InsertAsync(delPlayer); // add the player to the players_deleted table

            await db.DeleteAsync(player); // delete the player from the players table

            // Delete player redis data
            var key = "player:" + player.Id;
            await CacheManager.Instance.Del(key);

            key = "players:" + connection.AccountId;
            var list = CacheManager.Instance.CreateList<Guid>(key);
            await list.Rem(1, player.Id);

            // Delete items in redis cache

            //for (byte i = (byte)WindowType.Inventory; i < (byte) WindowType.Inventory; i++)
            {
                var items = Item.GetItems(player.Id, (byte) WindowType.Inventory);

                await foreach (var item in items)
                {
                    key = "item:" + item.Id;
                    await CacheManager.Instance.Del(key);
                }

                key = "items:" + player.Id + ":" + (byte) WindowType.Inventory;
                await CacheManager.Instance.Del(key);
            }

            // Delete all items in db
            await db.QueryAsync("DELETE FROM items WHERE PlayerId=@PlayerId", new { PlayerId = player.Id });
        }

        [Listener(typeof(CreateCharacter))]
        public static async Task OnCreateCharacter(this GameConnection connection, CreateCharacter packet)
        {
            Log.Debug($"Create character in slot {packet.Slot}");
            if (connection.AccountId == null)
            {
                connection.Close();
                Log.Warning($"Character create received before authorization");
                return;
            }

            var accountId = connection.AccountId ?? default;

            var db = DatabaseManager.GetGameDatabase();
            var count = await db.QuerySingleAsync<int>("SELECT COUNT(*) FROM players WHERE Name = @Name", new {Name = packet.Name});
            if (count > 0)
            {
                await connection.Send(new CreateCharacterFailure());
                return;
            }

            var job = JobInfo.Get((byte)packet.Class);
            
            // Create player data
            var player = new Player
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Name = packet.Name,
                PlayerClass = (byte) packet.Class,
                PositionX = 958870,
                PositionY = 272788,
                St = job.St,
                Iq = job.Iq, 
                Dx = job.Dx, 
                Ht = job.Ht,
                Health =  job.StartHp, 
                Mana = job.StartSp,
            };


            // Persist player
            await DatabaseManager.GetGameDatabase().InsertAsync(player);
            
            // Add player to cache
            await CacheManager.Instance.Set("player:" + player.Id, player);
            
            // Add player to the list of characters
            var list = CacheManager.Instance.CreateList<Guid>("players:" + accountId);
            var idx = await list.Push(player.Id);
            
            // Query responsible host for the map
            var host = World.World.Instance.GetMapHost(player.PositionX, player.PositionY);
            
            // Send success response
            var character = Character.FromEntity(player);
            character.Ip = IpUtils.ConvertIpToUInt(host.Ip);
            character.Port = host.Port;
            await connection.Send(new CreateCharacterSuccess
            {
                Slot = (byte)(idx - 1),
                Character = character
            });
        }
    }
}