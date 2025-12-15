using EnumsNET;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types.Skills;
using QuantumCore.Game.Skills;

namespace QuantumCore.Game.Commands;

[Command("all_skills_master", "Set all skills to master level for yourself")]
public class AllSkillsMasterCommand : ICommandHandler
{
    public Task ExecuteAsync(CommandContext context)
    {
        foreach (var skill in Enums.GetValues<ESkill>())
        {
            if (context.Player.Skills.CanUse(skill))
            {
                context.Player.Skills.SetLevel(skill, PlayerSkills.SKILL_MAX_LEVEL);
            }
        }

        context.Player.Skills.Send();

        return Task.CompletedTask;
    }
}
