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
    private readonly IAccountStore _accountStore;

    public DeleteCharacterHandler(ILogger<DeleteCharacterHandler> logger, IPlayerManager playerManager, IAccountStore accountStore)
    {
        _logger = logger;
        _playerManager = playerManager;
        _accountStore = accountStore;
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

        var account = await _accountStore.FindByIdAsync(accountId.Value);
        
        if (account is null) {
            ctx.Connection.Close();
            _logger.LogWarning("Account was not found");
            return;
        }

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
            await ctx.Connection.Send(new DeleteCharacterFail());
            return;
        }

        await _playerManager.DeletePlayerAsync(player);
        
        await ctx.Connection.Send(new DeleteCharacterSuccess
        {
            Slot = ctx.Packet.Slot
        });
    }
}