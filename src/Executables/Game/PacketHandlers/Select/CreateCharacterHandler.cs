using Microsoft.Extensions.Logging;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Select;

[PacketHandler(typeof(CreateCharacter))]
public class CreateCharacterHandler
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

    public void Execute(GamePacketContext ctx, CreateCharacter packet)
    {
        _logger.LogDebug("Create character in slot {Slot}", packet.Slot);

        var accountId = ctx.Connection.AccountId;
        if (accountId is null)
        {
            ctx.Connection.Close();
            _logger.LogWarning("Character create received before authorization");
            return;
        }

        var isNameInUse = await _playerManager.IsNameInUseAsync(packet.Name);
        if (isNameInUse)
        {
            ctx.Connection.Send(new CreateCharacterFailure(0));
            return;
        }


        var player =
            await _playerManager.CreateAsync(accountId.Value, packet.Name, (byte) packet.Class, packet.Appearance);
        // Query responsible host for the map
        var host = _world.GetMapHost(player.PositionX, player.PositionY);

        // Send success response
        var character = player.ToCharacter(IpUtils.ConvertIpToUInt(host.Ip), host.Port);
        ctx.Connection.Send(new CreateCharacterSuccess(player.Slot, character));
    }
}