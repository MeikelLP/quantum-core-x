using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.World;
using QuantumCore.API.PluginTypes;
using QuantumCore.Caching;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Packets;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.PlayerUtils;

namespace QuantumCore.Game.PacketHandlers.Select;

public class CreateCharacterHandler : IGamePacketHandler<CreateCharacter>
{
    private readonly ILogger<CreateCharacterHandler> _logger;
    private readonly IJobManager _jobManager;
    private readonly ICacheManager _cacheManager;
    private readonly IWorld _world;
    private readonly IDbConnection _db;

    public CreateCharacterHandler(ILogger<CreateCharacterHandler> logger, 
        IJobManager jobManager, ICacheManager cacheManager, IWorld world, IDbConnection db)
    {
        _logger = logger;
        _jobManager = jobManager;
        _cacheManager = cacheManager;
        _world = world;
        _db = db;
    }
    
    public async Task ExecuteAsync(GamePacketContext<CreateCharacter> ctx, CancellationToken token = default)
    {
        _logger.LogDebug("Create character in slot {Slot}", ctx.Packet.Slot);
        if (ctx.Connection.AccountId == null)
        {
            ctx.Connection.Close();
            _logger.LogWarning("Character create received before authorization");
            return;
        }

        var accountId = ctx.Connection.AccountId ?? default;

        var count = await _db.QuerySingleAsync<int>("SELECT COUNT(*) FROM players WHERE Name = @Name", new {Name = ctx.Packet.Name});
        if (count > 0)
        {
            await ctx.Connection.Send(new CreateCharacterFailure());
            return;
        }

        var job = _jobManager.Get((byte)ctx.Packet.Class);
        
        // Create player data
        var player = new Player
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Name = ctx.Packet.Name,
            PlayerClass = (byte) ctx.Packet.Class,
            PositionX = 958870,
            PositionY = 272788,
            St = job.St,
            Iq = job.Iq, 
            Dx = job.Dx, 
            Ht = job.Ht,
            Health =  job.StartHp, 
            Mana = job.StartSp
        };


        // Persist player
        await _db.InsertAsync(player);
        
        // Add player to cache
        await _cacheManager.Set("player:" + player.Id, player);
        
        // Add player to the list of characters
        var list = _cacheManager.CreateList<Guid>("players:" + accountId);
        var idx = await list.Push(player.Id);
        
        // Query responsible host for the map
        var host = _world.GetMapHost(player.PositionX, player.PositionY);
        
        // Send success response
        var character = Character.FromEntity(player);
        character.Ip = IpUtils.ConvertIpToUInt(host.Ip);
        character.Port = host.Port;
        await ctx.Connection.Send(new CreateCharacterSuccess
        {
            Slot = (byte)(idx - 1),
            Character = character
        });
    }
}