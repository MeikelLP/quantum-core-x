using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuantumCore.API.Game.Types;
using QuantumCore.Auth.Cache;
using QuantumCore.Cache;
using QuantumCore.Core.Networking;
using QuantumCore.Core.Utils;
using QuantumCore.Database;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers
{
    public class TokenLoginHandler : IPacketHandler<TokenLogin>
    {
        private readonly IDatabaseManager _databaseManager;
        private readonly ILogger<TokenLoginHandler> _logger;

        public TokenLoginHandler(IDatabaseManager databaseManager, ILogger<TokenLoginHandler> logger)
        {
            _databaseManager = databaseManager;
            _logger = logger;
        }
        
        public async Task ExecuteAsync(PacketContext<TokenLogin> ctx, CancellationToken cancellationToken = default)
        {
            var key = "token:" + ctx.Packet.Key;

            if (await CacheManager.Instance.Exists(key) <= 0)
            {
                _logger.LogWarning($"Received invalid auth token {ctx.Packet.Key} / {ctx.Packet.Username}");
                ctx.Connection.Close();
                return;
            }
            
            // Verify that the given token is for the given user
            var token = await CacheManager.Instance.Get<Token>(key);
            if (!string.Equals(token.Username, ctx.Packet.Username, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Received invalid auth token, username does not match {TokenUsername} != {PacketUserName}", token.Username, ctx.Packet.Username);
                ctx.Connection.Close();
                return;
            }
            
            // todo verify ip address
            
            _logger.LogDebug("Received valid auth token");
            
            // Remove TTL from token so we can use it for another game core transition
            await CacheManager.Instance.Persist(key);

            // Store the username and id for later reference
            ctx.Connection.Username = token.Username;
            ctx.Connection.AccountId = token.AccountId;
            
            _logger.LogDebug("Logged in user {UserName} ({AccountId})", token.Username, token.AccountId);
            
            // Load players of account
            var characters = new Characters();
            var i = 0;
            await foreach (var player in Player.GetPlayers(_databaseManager, token.AccountId).WithCancellation(cancellationToken))
            {
                var host = World.World.Instance.GetMapHost(player.PositionX, player.PositionY);
                
                // todo character slot position
                characters.CharacterList[i] = Character.FromEntity(player);
                characters.CharacterList[i].Ip = IpUtils.ConvertIpToUInt(host.Ip);
                characters.CharacterList[i].Port = host.Port;

                i++;
            }

            // Send empire to the client and characters
            await ctx.Connection.Send(new Empire { EmpireId = 1 }); // todo read from database
            await ctx.Connection.SetPhase(EPhases.Select);
            await ctx.Connection.Send(characters);
        }
    }
}