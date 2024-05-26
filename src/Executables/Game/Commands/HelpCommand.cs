using QuantumCore.API;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands;

[Command("help", "Shows this help message")]
[CommandNoPermission]
public static class HelpCommand
{
    [CommandMethod]
    public static Task Help(IPlayerEntity player, ICommandManager commandManager, int page = 1)
    {
        throw new NotImplementedException();
    }

    [CommandMethod("Shows an help with a specific command")]
    public static Task HelpWithCommand(IPlayerEntity player, ICommandManager commandManager, string command)
    {
        throw new NotImplementedException();
    }
}