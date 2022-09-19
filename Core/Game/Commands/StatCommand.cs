using System.Threading.Tasks;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands;

[Command("stat", "Adds a status point")]
[CommandNoPermission]
public class StatCommand
{
    [CommandMethod]
    public static async Task Execute(IPlayerEntity player, string status)
    {
        EPoints point;
        switch (status)
        {
            case "st":
                point = EPoints.St;
                break;
            case "dx":
                point = EPoints.Dx;
                break;
            case "ht":
                point = EPoints.Ht;
                break;
            case "iq":
                point = EPoints.Iq;
                break;
            default:
                return;
        }

        if (player.GetPoint(EPoints.StatusPoints) <= 0)
        {
            return;
        }
        
        await player.AddPoint(point, 1);
        await player.AddPoint(EPoints.StatusPoints, -1);
        await player.SendPoints();
    }
}