using CommandLine;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Skills;
using QuantumCore.API.Game.Types.Skills;

namespace QuantumCore.Game.Commands;

[Command("setskill", "Set a skill level for yourself")]
public class SetSkillCommand : ICommandHandler<SetSkillCommandOptions>
{
    public Task ExecuteAsync(CommandContext<SetSkillCommandOptions> context)
    {
        context.Player.Skills.SetLevel(context.Arguments.SkillId, context.Arguments.Level);
        context.Player.Skills.Send();

        return Task.CompletedTask;
    }
}

public class SetSkillCommandOptions
{
    [Value(1, Required = true)] public ESkillIndexes SkillId { get; set; }
    [Value(2, Required = true)] public byte Level { get; set; }
}
