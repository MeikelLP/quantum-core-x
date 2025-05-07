using CommandLine;
using QuantumCore.API.Game;
using QuantumCore.Game.Skills;

namespace QuantumCore.Game.Commands;

[Command("setjob", "Sets the job (skill group) of current player")]
public class SetJobCommand : ICommandHandler<SetJobCommandOptions>
{
    public Task ExecuteAsync(CommandContext<SetJobCommandOptions> context)
    {
        if (context.Arguments.Job is 0 or > PlayerSkills.SkillGroupMaxNum)
        {
            context.Player.SendChatInfo("Job not valid");
            return Task.CompletedTask;
        }

        var player = context.Player;
        var job = context.Arguments.Job;

        player.Skills.SetSkillGroup(job);

        return Task.CompletedTask;
    }
}

public class SetJobCommandOptions
{
    [Value(0, Required = true)] public byte Job { get; set; } = 0;
}
