using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands
{
    [Command("debug_damage", "Print debug information regarding damage calculation")]
    public class DebugCommandDamage
    {
        [CommandMethod]
        public static void Command(IPlayerEntity iplayer)
        {
            if (iplayer is not PlayerEntity player)
            {
                return;
            }
            
            var minWeapon = player.GetPoint(EPoints.MinWeaponDamage);
            var maxWeapon = player.GetPoint(EPoints.MaxWeaponDamage);
            var minAttack = player.GetPoint(EPoints.MinAttackDamage);
            var maxAttack = player.GetPoint(EPoints.MaxAttackDamage);
            player.SendChatMessage($"Weapon Damage: {minWeapon}-{maxWeapon}");
            player.SendChatMessage($"Attack Damage: {minAttack}-{maxAttack}");
        }
    }
}