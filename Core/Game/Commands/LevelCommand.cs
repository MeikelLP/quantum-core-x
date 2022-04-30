using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands;

[Command("level", "Sets the level of the current player or of another player")]
public class LevelCommand
{
    [CommandMethod]
    public static void SetMyLevel(IPlayerEntity player, byte level)
    {
        player.SetPoint(EPoints.Level, level);
        player.SendPoints();
    }

    [CommandMethod]
    public static void SetOtherLevel(IPlayerEntity player, IPlayerEntity target, byte level)
    {
        target.SetPoint(EPoints.Level, level);
        target.SendPoints();
    }
}