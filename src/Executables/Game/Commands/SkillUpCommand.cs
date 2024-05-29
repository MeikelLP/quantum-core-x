using CommandLine;
using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands;

[Command("skillup", "Levels up a skill")]
public class SkillUpCommand : ICommandHandler<SkillUpCommandOptions>
{
    public Task ExecuteAsync(CommandContext<SkillUpCommandOptions> context)
    {
        var player = context.Player;
        var skill = context.Arguments.SkillId;

        if (!player.Skills.CanUse(skill)) return Task.CompletedTask;

        player.Skills.SkillUp(skill);

        return Task.CompletedTask;
    }
}

public class SkillUpCommandOptions
{
    [Value(0, Required = true)] public uint SkillId { get; set; } = 0;
}
