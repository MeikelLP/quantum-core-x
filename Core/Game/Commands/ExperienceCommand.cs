using System.Threading.Tasks;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands;

[Command("exp", "Gives experience to a player")]
public class ExperienceCommand
{
    [CommandMethod]
    public static async Task Self(IPlayerEntity player, int experience)
    {
        await player.AddPoint(EPoints.Experience, experience);
        await player.SendPoints();
    }

    [CommandMethod]
    public static async Task Other(IPlayerEntity player, IPlayerEntity target, int experience)
    {
        await target.AddPoint(EPoints.Experience, experience);
        await target.SendPoints();
    }
}