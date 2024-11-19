using CommandLine;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Guild;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets.Guild;

namespace QuantumCore.Game.Commands.Guild;

[Command("guild_create", "Create guild")]
[CommandNoPermission]
public class GuildCreateCommand : ICommandHandler<GuildCreateCommandOptions>
{
    private readonly IGuildManager _guildManager;

    public GuildCreateCommand(IGuildManager guildManager)
    {
        _guildManager = guildManager;
    }

    public async Task ExecuteAsync(CommandContext<GuildCreateCommandOptions> context)
    {
        if (context.Arguments.Name is null or {Length: <= 0 or > 12})
        {
            context.Player.SendChatInfo("Guild name must not be null, empty or longer than 12");
            return;
        }

        if (await _guildManager.GetGuildByNameAsync(context.Arguments.Name) != null)
        {
            context.Player.SendChatInfo("Guild name already in use");
            return;
        }

        if (context.Player.Player.GuildId is not null)
        {
            context.Player.SendChatInfo("Player already in a guild");
            return;
        }

        var player = context.Player.Player;
        var guild = await _guildManager.CreateGuildAsync(context.Arguments.Name, player.Id);
        foreach (var nearbyPlayer in context.Player.GetNearbyPlayers())
        {
            nearbyPlayer.Connection.Send(new GuildName
            {
                Id = guild.Id,
                Name = guild.Name
            });
        }

        await context.Player.RefreshGuildAsync();
    }
}

public class GuildCreateCommandOptions
{
    [Value(0)] public string? Name { get; set; }
}