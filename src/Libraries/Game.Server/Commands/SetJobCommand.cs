using CommandLine;
using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands;

[Command("setjob", "Sets the job (skill group) of current player")]
public class SetJobCommand : ICommandHandler<SetJobCommandOptions>
{
    public Task ExecuteAsync(CommandContext<SetJobCommandOptions> context)
    {
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