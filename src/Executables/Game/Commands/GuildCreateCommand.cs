using CommandLine;
using QuantumCore.API;
using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands;

[Command("guild_create", "Create guild")]
public class GuildCreateCommand : ICommandHandler<GuildCreateCommandOptions>
{
    private readonly IGuildManager _guildManager;
    private readonly IPlayerManager _playerManager;

    public GuildCreateCommand(IGuildManager guildManager, IPlayerManager playerManager)
    {
        _guildManager = guildManager;
        _playerManager = playerManager;
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

        var player = context.Player.Player;
        await _guildManager.CreateGuildAsync(context.Arguments.Name, player.Id);
        await context.Player.RefreshGuildAsync();
    }
}

public class GuildCreateCommandOptions
{
    [Value(0)] public string? Name { get; set; }
}
