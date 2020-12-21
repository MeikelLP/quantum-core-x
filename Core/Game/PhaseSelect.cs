using System;
using System.Threading;
using Dapper;
using Dapper.Contrib.Extensions;
using QuantumCore.Cache;
using QuantumCore.Core.Constants;
using QuantumCore.Core.Utils;
using QuantumCore.Database;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World;
using QuantumCore.Game.World.Entities;
using Serilog;

namespace QuantumCore.Game
{
    public static class PhaseSelect
    {
        public static async void OnSelectCharacter(this GameConnection connection, SelectCharacter packet)
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
            connection.SetPhase(EPhases.Loading);
            
            // Load player
            var player = await Player.GetPlayer(accountId, packet.Slot);
            var entity = new PlayerEntity(player, connection);
            await entity.Load();
            
            connection.Player = entity;

            // Send information about the player to the client
            entity.SendBasicData();
            entity.SendPoints();
        }

        public static async void OnCreateCharacter(this GameConnection connection, CreateCharacter packet)
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
                connection.Send(new CreateCharacterFailure());
                return;
            }

            // Create player data
            var player = new Player
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Name = packet.Name,
                PlayerClass = (byte) packet.Class,
                PositionX = 958870,
                PositionY = 272788,
            };

            // Persist player
            await DatabaseManager.GetGameDatabase().InsertAsync(player);
            
            // Add player to cache
            var redis = CacheManager.Redis;
            await redis.Set("player:" + player.Id, player);
            
            // Add player to the list of characters
            var list = redis.CreateList<Guid>("players:" + accountId);
            var idx = await list.Push(player.Id);
            
            // Send success response
            var character = Character.FromEntity(player);
            character.Ip = IpUtils.ConvertIpToUInt(IpUtils.PublicIP);
            character.Port = 13001;
            connection.Send(new CreateCharacterSuccess
            {
                Slot = (byte)(idx - 1),
                Character = character
            });
        }
    }
}