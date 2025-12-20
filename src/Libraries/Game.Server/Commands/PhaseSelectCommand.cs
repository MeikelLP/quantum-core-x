using Microsoft.Extensions.DependencyInjection;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Guild;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Extensions;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.Systems.Events;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands;

[Command("phase_select", "Go back to character selection")]
[CommandNoPermission]
public class PhaseSelectCommand : ICommandHandler
{
    private readonly IWorld _world;
    private readonly IServiceScopeFactory _scopeFactory;

    public PhaseSelectCommand(IWorld world, IServiceScopeFactory scopeFactory)
    {
        _world = world;
        _scopeFactory = scopeFactory;
    }

    public Task ExecuteAsync(CommandContext context)
    {
        if (context.Player is not PlayerEntity { Events: var events } player)
        {
            throw new NotImplementedException();
        }

        // toggle mechanism
        if (events.Cancel(events.SafeLogoutCountdown))
        {
            player.SendChatInfo("Your logout has been cancelled.");
            return Task.CompletedTask;
        }

        if (player.Connection.AccountId is null) return Task.CompletedTask;

        player.SendChatInfo("Going back to character selection. Please wait.");

        events.Schedule(events.SafeLogoutCountdown, new SafeLogoutCountdownEvent.Args(
            "{0} seconds until character selection.",
            async () =>
            {
                await player.CalculatePlayedTimeAsync();
                await _world.DespawnPlayerAsync(player);
                player.Connection.SetPhase(EPhase.SELECT);

                var characters = new Characters();
                await using var scope = _scopeFactory.CreateAsyncScope();
                var playerManager = scope.ServiceProvider.GetRequiredService<IPlayerManager>();
                var guildManager = scope.ServiceProvider.GetRequiredService<IGuildManager>();
                var charactersFromCacheOrDb = await playerManager.GetPlayers(player.Connection.AccountId.Value);
                foreach (var playerData in charactersFromCacheOrDb)
                {
                    var host = _world.GetMapHost(playerData.PositionX, playerData.PositionY);
                    var guild = await guildManager.GetGuildForPlayerAsync(playerData.Id);

                    var slot = (int)playerData.Slot;
                    characters.CharacterList[slot] = playerData.ToCharacter();
                    characters.CharacterList[slot].Ip = BitConverter.ToInt32(host._ip.GetAddressBytes());
                    characters.CharacterList[slot].Port = host._port;
                    characters.GuildIds[slot] = guild?.Id ?? 0;
                    characters.GuildNames[slot] = guild?.Name ?? "";
                }

                player.Connection.Send(characters);
            }));

        return Task.CompletedTask;
    }
}
