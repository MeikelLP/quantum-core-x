using System.Threading.Tasks;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands;

[Command("level", "Sets the level of the current player or of another player")]
public class LevelCommand
{
    [CommandMethod]
    public static async Task SetMyLevel(IPlayerEntity player, byte level)
    {
        await player.SetPoint(EPoints.Level, level);
        await player.SendPoints();
    }

    [CommandMethod]
    public static async Task SetOtherLevel(IPlayerEntity player, IPlayerEntity target, byte level)
    {
        await target.SetPoint(EPoints.Level, level);
        await target.SendPoints();
    }
}