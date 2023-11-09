using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Auth.Persistence;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Select;

public class DeleteCharacterHandler : IGamePacketHandler<DeleteCharacter>
{
    private readonly ILogger<DeleteCharacterHandler> _logger;
    private readonly IPlayerManager _playerManager;
    private readonly IAccountRepository _accountRepository;

    public DeleteCharacterHandler(ILogger<DeleteCharacterHandler> logger, IPlayerManager playerManager, IAccountRepository accountRepository)
    {
        _logger = logger;
        _playerManager = playerManager;
        _accountRepository = accountRepository;
    }

    public async Task ExecuteAsync(GamePacketContext<DeleteCharacter> ctx, CancellationToken token = default)
    {
        _logger.LogDebug("Deleting character in slot {Slot}", ctx.Packet.Slot);

        var accountId = ctx.Connection.AccountId;
        if (accountId is null)
        {
            ctx.Connection.Close();
            _logger.LogWarning("Character remove received before authorization");
            return;
        }

        var account = await _accountRepository.FindByIdAsync(accountId.Value);

        if (account is null) {
            ctx.Connection.Close();
            _logger.LogWarning("Account was not found");
            return;
        }

        ctx.Connection.Send(new DeleteCharacterSuccess
        {
            Slot = ctx.Packet.Slot
        });

        var player = await _playerManager.GetPlayer(accountId.Value, ctx.Packet.Slot);
        if (player == null)
        {
            ctx.Connection.Close();
            _logger.LogWarning("Invalid or not exist character");
            return;
        }

        var sentDeleteCode = ctx.Packet.Code[..^1];
        if (account.DeleteCode != sentDeleteCode)
        {
            _logger.LogInformation("Account {AccountId} tried to delete player {PlayerId} but provided an invalid delete code", accountId, player.Id);
            ctx.Connection.Send(new DeleteCharacterFail());
            return;
        }

        await _playerManager.DeletePlayerAsync(player);

        ctx.Connection.Send(new DeleteCharacterSuccess
        {
            Slot = ctx.Packet.Slot
        });
    }
}
