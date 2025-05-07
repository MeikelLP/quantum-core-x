﻿using EnumsNET;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Skills;
using QuantumCore.Game.Skills;

namespace QuantumCore.Game.Commands;

[Command("all_skills_master", "Set all skills to master level for yourself")]
public class AllSkillsMasterCommand : ICommandHandler
{
    public Task ExecuteAsync(CommandContext context)
    {
        foreach (var skill in Enums.GetValues<ESkillIndexes>())
        {
            if (context.Player.Skills.CanUse(skill))
            {
                context.Player.Skills.SetLevel(skill, PlayerSkills.SkillMaxLevel);
            }
        }

        context.Player.Skills.Send();

        return Task.CompletedTask;
    }
}
