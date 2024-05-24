using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;

namespace QuantumCore.Game.Commands;

[Command("debug_damage", "Print debug information regarding damage calculation")]
public class DebugCommandDamage : ICommandHandler
{
    public Task ExecuteAsync(CommandContext context)
    {
        var minWeapon = context.Player.GetPoint(EPoints.MinWeaponDamage);
        var maxWeapon = context.Player.GetPoint(EPoints.MaxWeaponDamage);
        var minAttack = context.Player.GetPoint(EPoints.MinAttackDamage);
        var maxAttack = context.Player.GetPoint(EPoints.MaxAttackDamage);
        context.Player.SendChatMessage($"Weapon Damage: {minWeapon}-{maxWeapon}");
        context.Player.SendChatMessage($"Attack Damage: {minAttack}-{maxAttack}");

        return Task.CompletedTask;
    }
}