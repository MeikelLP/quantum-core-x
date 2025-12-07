using CommandLine;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Skills;
using QuantumCore.API.Game.Types.Skills;

namespace QuantumCore.Game.Commands;

[Command("setskillother", "Set a skill level for another player")]
public class SetSkillOtherCommand : ICommandHandler<SetSkillOtherCommandOptions>
{
    public Task ExecuteAsync(CommandContext<SetSkillOtherCommandOptions> context)
    {
        var target = context.Player.Map?.World.GetPlayer(context.Arguments.Target);
        if (target is null)
        {
            context.Player.SendChatMessage("Target player not found");
            return Task.CompletedTask;
        }

        target.Skills.SetLevel(context.Arguments.SkillId, context.Arguments.Level);
        target.Skills.Send();

        return Task.CompletedTask;
    }
}

public class SetSkillOtherCommandOptions
{
    [Value(0, Required = true)] public string Target { get; set; } = "";
    [Value(1, Required = true)] public ESkillIndexes SkillId { get; set; }
    [Value(2, Required = true)] public byte Level { get; set; }
}
