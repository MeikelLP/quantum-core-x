using CommandLine;
using EnumsNET;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types.Skills;

namespace QuantumCore.Game.Commands;

[Command("setjob", "Sets the job (skill group) of current player")]
public class SetJobCommand : ICommandHandler<SetJobCommandOptions>
{
    public Task ExecuteAsync(CommandContext<SetJobCommandOptions> context)
    {
        if (!Enums.TryToObject<ESkillGroup>(context.Arguments.Job, out var job, EnumValidation.IsDefined))
        {
            context.Player.SendChatInfo("Job not valid");
            return Task.CompletedTask;
        }

        context.Player.Skills.SetSkillGroup(job);

        return Task.CompletedTask;
    }
}

public class SetJobCommandOptions
{
    [Value(0, Required = true)] public byte Job { get; set; } = 0;
}
