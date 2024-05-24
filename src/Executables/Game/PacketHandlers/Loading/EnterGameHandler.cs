using Microsoft.Extensions.Logging;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Caching;
using QuantumCore.Extensions;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Loading;

[PacketHandler(typeof(EnterGame))]
public class EnterGameHandler
{
    private readonly ILogger<EnterGameHandler> _logger;
    private readonly IWorld _world;
    private readonly ICacheManager _cache;

    public EnterGameHandler(ILogger<EnterGameHandler> logger, IWorld world, ICacheManager cache)
    {
        _logger = logger;
        _world = world;
        _cache = cache;
    }

    public void Execute(GamePacketContext ctx, EnterGame packet)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            _logger.LogWarning("Trying to enter game without a player!");
            ctx.Connection.Close();
            return;
        }

        // Enable game phase
        ctx.Connection.SetPhase(EPhases.Game);

        ctx.Connection.Send(new GameTime((uint) ctx.Connection.Server.ServerTime));
        ctx.Connection.Send(new Channel(1)); // tod

        var key = $"player:{player.Player.Id}:loggedInTime";
        await _cache.Set(key, ctx.Connection.Server.ServerTime);

        player.ShowEntity(ctx.Connection);
        _world.SpawnEntity(player);

        player.SendInventory();
    }
}