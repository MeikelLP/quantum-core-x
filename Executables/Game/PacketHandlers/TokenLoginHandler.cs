using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.API.PluginTypes;
using QuantumCore.Auth.Cache;
using QuantumCore.Caching;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers
{
    public class TokenLoginHandler : IGamePacketHandler<TokenLogin>
    {
        private readonly IDbConnection _db;
        private readonly ILogger<TokenLoginHandler> _logger;
        private readonly ICacheManager _cacheManager;
        private readonly IWorld _world;
        private readonly IPlayerFactory _playerFactory;

        public TokenLoginHandler(IDbConnection db, ILogger<TokenLoginHandler> logger, ICacheManager cacheManager, IWorld world, IPlayerFactory playerFactory)
        {
            _db = db;
            _logger = logger;
            _cacheManager = cacheManager;
            _world = world;
            _playerFactory = playerFactory;
        }
        
        public async Task ExecuteAsync(GamePacketContext<TokenLogin> ctx, CancellationToken cancellationToken = default)
        {
            var key = "token:" + ctx.Packet.Key;

            if (await _cacheManager.Exists(key) <= 0)
            {
                _logger.LogWarning("Received invalid auth token {Key} / {Username}", ctx.Packet.Key, ctx.Packet.Username);
                ctx.Connection.Close();
                return;
            }
            
            // Verify that the given token is for the given user
            var token = await _cacheManager.Get<Token>(key);
            if (!string.Equals(token.Username, ctx.Packet.Username, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Received invalid auth token, username does not match {TokenUsername} != {PacketUserName}", token.Username, ctx.Packet.Username);
                ctx.Connection.Close();
                return;
            }
            
            // todo verify ip address
            
            _logger.LogDebug("Received valid auth token");
            
            // Remove TTL from token so we can use it for another game core transition
            await _cacheManager.Persist(key);

            // Store the username and id for later reference
            ctx.Connection.Username = token.Username;
            ctx.Connection.AccountId = token.AccountId;
            
            _logger.LogDebug("Logged in user {UserName} ({AccountId})", token.Username, token.AccountId);
            
            // Load players of account
            var characters = new Characters();
            var i = 0;
            var charactersFromCacheOrDb = await _playerFactory.GetPlayers(token.AccountId).ToArrayAsync(cancellationToken);
            foreach (var player in charactersFromCacheOrDb)
            {
                var host = _world.GetMapHost(player.PositionX, player.PositionY);
                
                // todo character slot position
                characters.CharacterList[i] = Character.FromEntity(player);
                characters.CharacterList[i].Ip = IpUtils.ConvertIpToUInt(host.Ip);
                characters.CharacterList[i].Port = host.Port;

                i++;
            }

            // Send empire to the client and characters
            var empire = await _db.QueryFirstOrDefaultAsync<byte>(
                "SELECT Empire FROM account.accounts WHERE Id = @AccountId", new {AccountId = token.AccountId});

            await ctx.Connection.Send(new Empire { EmpireId = empire });
            await ctx.Connection.SetPhaseAsync(EPhases.Select);
            await ctx.Connection.Send(characters);
        }
    }
}