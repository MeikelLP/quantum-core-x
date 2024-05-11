using Game.Caching;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Data;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.API.PluginTypes;
using QuantumCore.Auth.Cache;
using QuantumCore.Caching;
using QuantumCore.Core.Utils;
using QuantumCore.Extensions;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers
{
    public class TokenLoginHandler : IGamePacketHandler<TokenLogin>
    {
        private readonly ILogger<TokenLoginHandler> _logger;
        private readonly ICacheManager _cacheManager;
        private readonly IWorld _world;
        private readonly IEmpireRepository _empireRepository;
        private readonly IPlayerManager _playerManager;
        private readonly ICachePlayerRepository _playerCache;

        public TokenLoginHandler(ILogger<TokenLoginHandler> logger, ICacheManager cacheManager, IWorld world, IPlayerManager playerManager, ICachePlayerRepository playerCache)
        {
            _logger = logger;
            _cacheManager = cacheManager;
            _world = world;
            _empireRepository = empireRepository;
            _playerManager = playerManager;
            _playerCache = playerCache;
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
            var charactersFromCacheOrDb = await _playerManager.GetPlayers(token.AccountId);
            foreach (var player in charactersFromCacheOrDb)
            {
                var host = _world.GetMapHost(player.PositionX, player.PositionY);

                // todo character slot position
                characters.CharacterList[i] = player.ToCharacter();
                characters.CharacterList[i].Ip = IpUtils.ConvertIpToUInt(host.Ip);
                characters.CharacterList[i].Port = host.Port;

                i++;
            }

            // When there are no characters belonging to the account, the empire status is stored in the cache.
            byte empire = 0;
            if (charactersFromCacheOrDb.Length > 0)
            {
                empire = charactersFromCacheOrDb[0].Empire;
                await _cacheManager.Set($"account:{ctx.Connection.AccountId}:game:select:selected-player", charactersFromCacheOrDb[0].Id);
            }

            // TODO:: set player id to character?
            ctx.Connection.Send(new Empire { EmpireId = empire });
            ctx.Connection.SetPhase(EPhases.Select);
            ctx.Connection.Send(characters);

        }
    }
}
