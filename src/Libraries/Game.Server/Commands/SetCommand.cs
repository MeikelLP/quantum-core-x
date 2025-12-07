using CommandLine;
using EnumsNET;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;

namespace QuantumCore.Game.Commands;

[Command("set", "Set a point of a target player")]
public class SetCommand : ICommandHandler<SetCommandOptions>
{
    public Task ExecuteAsync(CommandContext<SetCommandOptions> context)
    {
        var target = context.Player.Map?.World.GetPlayer(context.Arguments.Target);
        if (target is null)
        {
            context.Player.SendChatInfo("Target not found");
            return Task.CompletedTask;
        }

        if (!Enums.TryParse<SetCommandType>(context.Arguments.Type, true, out var type))
        {
            context.Player.SendChatInfo(
                $"Set type not valid. Valid options include: {string.Join(", ", Enums.GetValues<SetCommandType>())}");
            return Task.CompletedTask;
        }

        switch (type)
        {
            case SetCommandType.Gold:
                target.SetPoint(EPoint.Gold, context.Arguments.Value);
                break;
            case SetCommandType.Exp:
                target.SetPoint(EPoint.Experience, context.Arguments.Value);
                break;
            case SetCommandType.Align:
                // TODO align not implemented yet
                break;
        }

        target.SendPoints();

        return Task.CompletedTask;
    }
}

public class SetCommandOptions
{
    [Value(0, Required = true)] public string Target { get; set; } = "";
    [Value(1, Required = true)] public string Type { get; set; } = "";
    [Value(2, Required = true)] public uint Value { get; set; }
}

public enum SetCommandType
{
    Gold = 1,
    Exp = 2,
    Align = 3
}
