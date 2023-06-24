using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.API.PluginTypes;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.PlayerUtils;

namespace QuantumCore.Game.PacketHandlers.Select;

public class CreateCharacterHandler : IGamePacketHandler<CreateCharacter>
{
    private readonly ILogger<CreateCharacterHandler> _logger;
    private readonly IJobManager _jobManager;
    private readonly IWorld _world;
    private readonly IPlayerManager _playerManager;

    public CreateCharacterHandler(ILogger<CreateCharacterHandler> logger, 
        IJobManager jobManager, IWorld world, IPlayerManager playerManager)
    {
        _logger = logger;
        _jobManager = jobManager;
        _world = world;
        _playerManager = playerManager;
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

        var isNameInUse = await _playerManager.IsNameInUseAsync(ctx.Packet.Name);
        if (isNameInUse)
        {
            await ctx.Connection.Send(new CreateCharacterFailure());
            return;
        }

        var job = _jobManager.Get((byte)ctx.Packet.Class);
        
        // Create player data
        var player = new PlayerData
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
            Health = job.StartHp, 
            Mana = job.StartSp
        };


        var slot = await _playerManager.CreateAsync(player);
        
        // Query responsible host for the map
        var host = _world.GetMapHost(player.PositionX, player.PositionY);
        
        // Send success response
        var character = player.ToCharacter();
        character.Ip = IpUtils.ConvertIpToUInt(host.Ip);
        character.Port = host.Port;
        await ctx.Connection.Send(new CreateCharacterSuccess
        {
            Slot = slot,
            Character = character
        });
    }
}