using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands;

[Command("gold", "Adds the given amount of gold")]
public static class GoldCommand
{
    [CommandMethod("Gives yourself the specified amount of gold, if amount is negative gold will be removed")]
    public static void GiveMyself(IPlayerEntity player, int amount)
    {
        GiveAnother(player, player, amount);
    }

    [CommandMethod("Gives the player the specified amount of gold, if amount is negative gold will be removed")]
    public static void GiveAnother(IPlayerEntity player, IPlayerEntity target, int amount)
    {
        if (target is PlayerEntity p)
        {
            p.AddPoint(EPoints.Gold, amount);
            p.SendPoints();
        }
    }
}