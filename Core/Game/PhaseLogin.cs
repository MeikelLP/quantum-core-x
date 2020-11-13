using System;
using System.Threading.Tasks;
using QuantumCore.Auth.Cache;
using QuantumCore.Cache;
using QuantumCore.Core.Constants;
using QuantumCore.Core.Utils;
using QuantumCore.Database;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World;
using Serilog;

namespace QuantumCore.Game
{
    public static class PhaseLogin
    {
        public static async void OnTokenLogin(this GameConnection connection, TokenLogin packet)
        {
            var key = "token:" + packet.Key;

            if (await CacheManager.Redis.Exists(key) <= 0)
            {
                Log.Warning($"Received invalid auth token {packet.Key} / {packet.Username}");
                connection.Close();
                return;
            }
            
            // Verify that the given token is for the given user
            var token = await CacheManager.Redis.Get<Token>(key);
            if (!string.Equals(token.Username, packet.Username, StringComparison.OrdinalIgnoreCase))
            {
                Log.Warning($"Received invalid auth token, username does not match {token.Username} != {packet.Username}");
                connection.Close();
                return;
            }
            
            // todo verify ip address
            
            Log.Debug("Received valid auth token");
            
            // Remove TTL from token so we can use it for another game core transition
            await CacheManager.Redis.Persist(key);

            // Store the username and id for later reference
            connection.Username = token.Username;
            connection.AccountId = token.AccountId;
            
            Log.Debug($"Logged in user {token.Username} ({token.AccountId})");
            
            // Load players of account
            var characters = new Characters();
            var i = 0;
            await foreach (var player in Player.GetPlayers(token.AccountId))
            {
                // todo character slot position
                characters.CharacterList[i] = Character.FromEntity(player);
                // todo calculate real target ip and port
                characters.CharacterList[i].Ip = IpUtils.ConvertIpToUInt("127.0.0.1");
                characters.CharacterList[i].Port = 13001;

                i++;
            }

            // Send empire to the client and characters
            connection.Send(new Empire { EmpireId = 1 }); // todo read from database
            connection.SetPhase(EPhases.Select);
            connection.Send(characters);
        }
        
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
            
            connection.SetPhase(EPhases.Loading);
            
            // Load player
            var player = await Player.GetPlayer(accountId, packet.Slot);
            var entity = new PlayerEntity(player, connection);
            connection.Player = entity;
            
            // Send information about the player to the client
            entity.SendBasicData();
            entity.SendPoints();
        }
    }
}