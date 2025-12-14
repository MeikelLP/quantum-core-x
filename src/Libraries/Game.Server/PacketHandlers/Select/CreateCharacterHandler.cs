using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.World;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Select;

public class CreateCharacterHandler : IGamePacketHandler<CreateCharacter>
{
    private readonly ILogger<CreateCharacterHandler> _logger;
    private readonly IWorld _world;
    private readonly IPlayerManager _playerManager;

    public CreateCharacterHandler(ILogger<CreateCharacterHandler> logger, IWorld world, IPlayerManager playerManager)
    {
        _logger = logger;
        _world = world;
        _playerManager = playerManager;
    }

    public async Task ExecuteAsync(GamePacketContext<CreateCharacter> ctx, CancellationToken token = default)
    {
        _logger.LogDebug("Create character in slot {Slot}", ctx.Packet.Slot);

        var accountId = ctx.Connection.AccountId;
        if (accountId is null)
        {
            ctx.Connection.Close();
            _logger.LogWarning("Character create received before authorization");
            return;
        }

        var isNameInUse = await _playerManager.IsNameInUseAsync(ctx.Packet.Name);
        if (isNameInUse)
        {
            ctx.Connection.Send(new CreateCharacterFailure());
            return;
        }


        var player = await _playerManager.CreateAsync(accountId.Value, ctx.Packet.Name, ctx.Packet.Class,
            ctx.Packet.Appearance);
        // Query responsible host for the map
        var host = _world.GetMapHost(player.PositionX, player.PositionY);

        // Send success response
        var character = player.ToCharacter();
        character.Ip = BitConverter.ToInt32(host.Ip.GetAddressBytes());
        character.Port = host.Port;
        ctx.Connection.Send(new CreateCharacterSuccess {Slot = player.Slot, Character = character});
    }
}
