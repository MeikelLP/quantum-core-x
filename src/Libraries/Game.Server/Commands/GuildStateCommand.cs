using CommandLine;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Guild;

namespace QuantumCore.Game.Commands;

[Command("gstate", "Get information about a guild")]
public class GuildStateCommand : ICommandHandler<GuildStateCommandOptions>
{
    private readonly IGuildManager _guildManager;

    public GuildStateCommand(IGuildManager guildManager)
    {
        _guildManager = guildManager;
    }

    public async Task ExecuteAsync(CommandContext<GuildStateCommandOptions> context)
    {
        var guild = await _guildManager.GetGuildByNameAsync(context.Arguments.Name);
        if (guild is null)
        {
            context.Player.SendChatInfo("Guild not found");
            return;
        }

        context.Player.SendChatInfo($"Guild ID: {guild.Id}");
        context.Player.SendChatInfo($"Guild Master ID: {guild.OwnerId}");
        // TODO implement guild is in war
    }
}

public class GuildStateCommandOptions
{
    [Value(0, Required = true)] public string Name { get; set; } = "";
}
