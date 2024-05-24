using Microsoft.Extensions.DependencyInjection;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Utils;
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
        context.Player.Connection.SetPhase(EPhases.Select);

        var i = 0;
        await using var scope = _serviceProvider.CreateAsyncScope();
        var playerManager = scope.ServiceProvider.GetRequiredService<IPlayerManager>();
        var charactersFromCacheOrDb = await playerManager.GetPlayers(context.Player.Connection.AccountId.Value);
        SendCharacters(context.Player.Connection, charactersFromCacheOrDb, i);
    }

    private void SendCharacters(IGameConnection connection, PlayerData[] charactersFromCacheOrDb, int i)
    {
        var characters = new Characters();
        foreach (var player in charactersFromCacheOrDb)
        {
            var host = _world.GetMapHost(player.PositionX, player.PositionY);

            // todo character slot position
            characters.CharacterList[i] = player.ToCharacter(IpUtils.ConvertIpToUInt(host.Ip), host.Port);

            i++;
        }

        connection.Send(characters);
    }
}