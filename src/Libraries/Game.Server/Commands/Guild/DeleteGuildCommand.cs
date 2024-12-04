using System.ComponentModel.DataAnnotations;
using CommandLine;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Guild;

namespace QuantumCore.Game.Commands.Guild;

[Command("deleteguild", "Delete guild")]
public class GuildDeleteCommand : ICommandHandler<GuildDeleteCommandOptions>
{
    private readonly IGuildManager _guildManager;

    public GuildDeleteCommand(IGuildManager guildManager)
    {
        _guildManager = guildManager;
    }

    public async Task ExecuteAsync(CommandContext<GuildDeleteCommandOptions> context)
    {
        var guild = await _guildManager.GetGuildByNameAsync(context.Arguments.GuildName);
        if (guild is null)
        {
            context.Player.SendChatInfo("Guild not found");
            return;
        }

        await _guildManager.RemoveGuildAsync(guild.Id);
        await context.Player.RefreshGuildAsync();
        // TODO update all members
    }
}

public class GuildDeleteCommandOptions
{
    [Value(0)] [Required] public string GuildName { get; set; } = "";
}
