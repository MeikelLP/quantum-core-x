using System;
using QuantumCore.Auth.Cache;
using QuantumCore.Cache;
using QuantumCore.Core.Constants;
using QuantumCore.Database;
using QuantumCore.Game.Packets;
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
            
            var token = await CacheManager.Redis.Get<Token>(key);
            if (!string.Equals(token.Username, packet.Username, StringComparison.OrdinalIgnoreCase))
            {
                Log.Warning($"Received invalid auth token, username does not match {token.Username} != {packet.Username}");
                connection.Close();
                return;
            }
            
            Log.Debug("Received valid auth token");
            
            // Remove TTL from token so we can use it for another game core transition
            await CacheManager.Redis.Persist(key);

            // Store the username and id for later reference
            connection.Username = token.Username;
            connection.AccountId = token.AccountId;
            
            Log.Debug($"Logged in user {token.Username} ({token.AccountId})");
            
            var characters = new Characters();
            var i = 0;
            // Load players of account
            await foreach (var player in Player.GetPlayers(connection.AccountId))
            {
                characters.CharacterList[i] = Character.FromEntity(player);

                i++;
            }

            connection.Send(new Empire { EmpireId = 1 });
            connection.SetPhase(EPhases.Select);
            connection.Send(characters);
        }
    }
}