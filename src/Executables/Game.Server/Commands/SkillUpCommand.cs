using CommandLine;
using Microsoft.Extensions.Logging;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Skills;

namespace QuantumCore.Game.Commands;

[Command("skillup", "Levels up a skill")]
public class SkillUpCommand : ICommandHandler<SkillUpCommandOptions>
{
    private readonly ILogger<SkillUpCommand> _logger;
    
    public SkillUpCommand(ILogger<SkillUpCommand> logger)
    {
        _logger = logger;
    }
    
    public Task ExecuteAsync(CommandContext<SkillUpCommandOptions> context)
    {
        var player = context.Player;
        
        if (!Enum.TryParse<ESkillIndexes>(context.Arguments.SkillId.ToString(), out var skill))
        {
            _logger.LogWarning("Skill with Id({SkillId}) not defined", context.Arguments.SkillId);
            return Task.CompletedTask;
        }

        if (player.Skills.CanUse(skill))
        {
            player.Skills.SkillUp(skill);
            return Task.CompletedTask;
        }

        switch (skill)
        {
            case ESkillIndexes.HoseWildAttack:
            case ESkillIndexes.HorseCharge:
            case ESkillIndexes.HorseEscape:
            case ESkillIndexes.HorseWildAttackRange:
                
            case ESkillIndexes.AddHp:
            case ESkillIndexes.PenetrationResistance:
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
