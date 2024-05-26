using Microsoft.Extensions.Logging;
using QuantumCore.API.Core.Models;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Select;

[PacketHandler(typeof(DeleteCharacter))]
public class DeleteCharacterHandler
{
    private readonly ILogger<DeleteCharacterHandler> _logger;
    private readonly IPlayerManager _playerManager;
    private readonly HttpClient _http;

    public DeleteCharacterHandler(ILogger<DeleteCharacterHandler> logger, IPlayerManager playerManager, HttpClient http)
    {
        _logger = logger;
        _playerManager = playerManager;
        _http = http;
    }

    public void Execute(GamePacketContext ctx, DeleteCharacter packet)
    {
        _logger.LogDebug("Deleting character in slot {Slot}", packet.Slot);

        var accountId = ctx.Connection.AccountId;
        if (accountId is null)
        {
            ctx.Connection.Close();
            _logger.LogWarning("Character remove received before authorization");
            return;
        }

        var account = await _http.GetFromJsonAsync<AccountData>($"/account/{accountId.Value}", token);

        if (account is null)
        {
            ctx.Connection.Close();
            _logger.LogWarning("Account was not found");
            return;
        }

        ctx.Connection.Send(new DeleteCharacterSuccess(packet.Slot));

        var player = await _playerManager.GetPlayer(accountId.Value, packet.Slot);
        if (player == null)
        {
            ctx.Connection.Close();
            _logger.LogWarning("Invalid or not exist character");
            return;
        }

        var sentDeleteCode = packet.Code[..^1];
        if (account.DeleteCode != sentDeleteCode)
        {
            _logger.LogInformation(
                "Account {AccountId} tried to delete player {PlayerId} but provided an invalid delete code", accountId,
                player.Id);
            ctx.Connection.Send(new DeleteCharacterFail());
            return;
        }

        await _playerManager.DeletePlayerAsync(player);

        ctx.Connection.Send(new DeleteCharacterSuccess(packet.Slot));
    }
}