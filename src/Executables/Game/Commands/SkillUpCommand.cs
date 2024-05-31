using CommandLine;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Skills;

namespace QuantumCore.Game.Commands;

[Command("skillup", "Levels up a skill")]
public class SkillUpCommand : ICommandHandler<SkillUpCommandOptions>
{
    public Task ExecuteAsync(CommandContext<SkillUpCommandOptions> context)
    {
        var player = context.Player;
        var skill = context.Arguments.SkillId;

        if (player.Skills.CanUse(skill))
        {
            player.Skills.SkillUp(skill);
            return Task.CompletedTask;
        }

        switch (skill)
        {
            case (uint) ESkillIndexes.HoseWildAttack:
            case (uint) ESkillIndexes.HorseCharge:
            case (uint) ESkillIndexes.HorseEscape:
            case (uint) ESkillIndexes.HorseWildAttackRange:
                
            case (uint) ESkillIndexes.AddHp:
            case (uint) ESkillIndexes.PenetrationResistance:
                player.Skills.SkillUp(skill);
                break;
        }

        return Task.CompletedTask;
    }
}

public class SkillUpCommandOptions
{
    [Value(0, Required = true)] public uint SkillId { get; set; } = 0;
}
