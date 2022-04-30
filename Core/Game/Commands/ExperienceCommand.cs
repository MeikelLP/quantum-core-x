using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands;

[Command("exp", "Gives experience to a player")]
public class ExperienceCommand
{
    [CommandMethod]
    public static void Self(IPlayerEntity player, int experience)
    {
        player.AddPoint(EPoints.Experience, experience);
        player.SendPoints();
    }

    [CommandMethod]
    public static void Other(IPlayerEntity player, IPlayerEntity target, int experience)
    {
        target.AddPoint(EPoints.Experience, experience);
        target.SendPoints();
    }
}