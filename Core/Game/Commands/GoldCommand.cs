using System.Threading.Tasks;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands;

[Command("gold", "Adds the given amount of gold")]
public static class GoldCommand
{
    [CommandMethod("Gives yourself the specified amount of gold, if amount is negative gold will be removed")]
    public static async Task GiveMyself(IPlayerEntity player, int amount)
    {
        await GiveAnother(player, player, amount);
    }

    [CommandMethod("Gives the player the specified amount of gold, if amount is negative gold will be removed")]
    public static async Task GiveAnother(IPlayerEntity player, IPlayerEntity target, int amount)
    {
        if (target is PlayerEntity p)
        {
            await p.AddPoint(EPoints.Gold, amount);
            await p.SendPoints();
        }
    }
}