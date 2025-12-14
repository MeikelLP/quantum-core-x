using QuantumCore.API.Game;
using QuantumCore.API.Game.Types.Entities;

namespace QuantumCore.Game.Commands;

[Command("debug_damage", "Print debug information regarding damage calculation")]
public class DebugCommandDamage : ICommandHandler
{
    public Task ExecuteAsync(CommandContext context)
    {
        var minWeapon = context.Player.GetPoint(EPoint.MIN_WEAPON_DAMAGE);
        var maxWeapon = context.Player.GetPoint(EPoint.MAX_WEAPON_DAMAGE);
        var minAttack = context.Player.GetPoint(EPoint.MIN_ATTACK_DAMAGE);
        var maxAttack = context.Player.GetPoint(EPoint.MAX_ATTACK_DAMAGE);
        context.Player.SendChatMessage($"Weapon Damage: {minWeapon}-{maxWeapon}");
        context.Player.SendChatMessage($"Attack Damage: {minAttack}-{maxAttack}");

        return Task.CompletedTask;
    }
}
