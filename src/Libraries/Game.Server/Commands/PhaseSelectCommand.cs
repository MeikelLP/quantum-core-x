using Microsoft.Extensions.DependencyInjection;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Guild;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Extensions;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.Commands;

[Command("phase_select", "Go back to character selection")]
[CommandNoPermission]
public class PhaseSelectCommand : ICommandHandler
{
    private readonly IWorld _world;
    private readonly IServiceProvider _serviceProvider;

    public PhaseSelectCommand(IWorld world, IServiceProvider serviceProvider)
    {
        _world = world;
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync(CommandContext context)
    {
        if (context.Player.Connection.AccountId is null) return;

        context.Player.SendChatInfo("Going back to character selection. Please wait.");

        // todo implement wait

        await context.Player.CalculatePlayedTimeAsync();

        await _world.DespawnPlayerAsync(context.Player);
        context.Player.Connection.SetPhase(EPhase.Select);

        var characters = new Characters();
        await using var scope = _serviceProvider.CreateAsyncScope();
        var playerManager = scope.ServiceProvider.GetRequiredService<IPlayerManager>();
        var guildManager = scope.ServiceProvider.GetRequiredService<IGuildManager>();
        var charactersFromCacheOrDb = await playerManager.GetPlayers(context.Player.Connection.AccountId.Value);
        foreach (var player in charactersFromCacheOrDb)
        {
            var host = _world.GetMapHost(player.PositionX, player.PositionY);
            var guild = await guildManager.GetGuildForPlayerAsync(player.Id);

            var slot = (int)player.Slot;
            characters.CharacterList[slot] = player.ToCharacter();
            characters.CharacterList[slot].Ip = BitConverter.ToInt32(host._ip.GetAddressBytes());
            characters.CharacterList[slot].Port = host._port;
            characters.GuildIds[slot] = guild?.Id ?? 0;
            characters.GuildNames[slot] = guild?.Name ?? "";
        }

        context.Player.Connection.Send(characters);
    }
}