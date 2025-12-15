using System.Text;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types.Entities;
using QuantumCore.Game.World;

namespace QuantumCore.Game.Commands;

[Command("state", "Shows stats and current information about your character")]
public class StateCommand : ICommandHandler
{
    public Task ExecuteAsync(CommandContext context)
    {
        var p = context.Player;
        var sb = new StringBuilder($"{p.Name}'s State: ");

        // TODO Check if in battle
        if (p.Dead)
        {
            sb.Append("Dead");
        }
        else
        {
            sb.Append("Standing");
        }

        // TODO check if is in shop
        // TODO check if is in trade with other player or npc
        context.Player.SendChatInfo(sb.ToString());
        sb.Clear();

        var globalX = p.PositionX;
        var globalY = p.PositionX;
        var localX = (uint)((p.PositionX - p.Map!.Position.X) / (float)Map.SPAWN_POSITION_MULTIPLIER);
        var localY = (uint)((p.PositionY - p.Map!.Position.Y) / (float)Map.SPAWN_POSITION_MULTIPLIER);
        sb.Append($"Coordinate {globalX}x{globalY} ({localX}x{localY}) Map {p.Map.Name}");

        context.Player.SendChatInfo(sb.ToString());
        sb.Clear();

        context.Player.SendChatInfo($"LEV {p.Player.Level}");
        context.Player.SendChatInfo($"HP {p.Health}/{p.Player.MaxHp}");
        context.Player.SendChatInfo($"SP {p.Mana}/{p.Player.MaxSp}");
        context.Player.SendChatInfo($"ATT {context.Player.GetPoint(EPoint.ATTACK_GRADE)} " +
                                    $"MAGIC_ATT {context.Player.GetPoint(EPoint.MAGIC_ATTACK_GRADE)} " +
                                    $"SPD {context.Player.GetPoint(EPoint.ATTACK_SPEED)} " +
                                    $"CRIT {context.Player.GetPoint(EPoint.CRITICAL_PERCENTAGE)}% " +
                                    $"PENE {context.Player.GetPoint(EPoint.PENETRATE_PERCENTAGE)}% " +
                                    $"ATT_BONUS {context.Player.GetPoint(EPoint.ATTACK_BONUS)}%");
        context.Player.SendChatInfo($"DEF {context.Player.GetPoint(EPoint.DEFENCE_GRADE)} " +
                                    $"MAGIC_DEF {context.Player.GetPoint(EPoint.MAGIC_DEFENCE_GRADE)} " +
                                    $"BLOCK {context.Player.GetPoint(EPoint.BLOCK)}% " +
                                    $"DODGE {context.Player.GetPoint(EPoint.DODGE)}% " +
                                    $"DEF_BONUS {context.Player.GetPoint(EPoint.DEFENCE_BONUS)}%");
        context.Player.SendChatInfo("RESISTANCES:");
        context.Player.SendChatInfo($"   WARR:{context.Player.GetPoint(EPoint.RESIST_WARRIOR)}% " +
                                    $"ASAS:{context.Player.GetPoint(EPoint.RESIST_ASSASSIN)}% " +
                                    $"SURA:{context.Player.GetPoint(EPoint.RESIST_SURA)}% " +
                                    $"SHAM:{context.Player.GetPoint(EPoint.RESIST_SHAMAN)}%");
        context.Player.SendChatInfo($"   SWORD:{context.Player.GetPoint(EPoint.RESIST_SWORD)}% " +
                                    $"THSWORD:{context.Player.GetPoint(EPoint.RESIST_TWO_HANDED)}% " +
                                    $"DAGGER:{context.Player.GetPoint(EPoint.RESIST_DAGGER)}% " +
                                    $"BELL:{context.Player.GetPoint(EPoint.RESIST_BELL)}% " +
                                    $"FAN:{context.Player.GetPoint(EPoint.RESIST_FAN)}% " +
                                    $"BOW:{context.Player.GetPoint(EPoint.RESIST_BOW)}%");
        context.Player.SendChatInfo($"   FIRE:{context.Player.GetPoint(EPoint.RESIST_FIRE)}% " +
                                    $"ELEC:{context.Player.GetPoint(EPoint.RESIST_ELECTRIC)}% " +
                                    $"MAGIC:{context.Player.GetPoint(EPoint.RESIST_MAGIC)}% " +
                                    $"WIND:{context.Player.GetPoint(EPoint.RESIST_WIND)}% " +
                                    $"CRIT:{context.Player.GetPoint(EPoint.RESIST_CRITICAL)}% " +
                                    $"PENE:{context.Player.GetPoint(EPoint.RESIST_PENETRATE)}%");
        context.Player.SendChatInfo($"   ICE:{context.Player.GetPoint(EPoint.RESIST_ICE)}% " +
                                    $"EARTH:{context.Player.GetPoint(EPoint.RESIST_EARTH)}% " +
                                    $"DARK:{context.Player.GetPoint(EPoint.RESIST_DARK)}%");

        context.Player.SendChatInfo("MALL:");
        context.Player.SendChatInfo($"   ATT:{context.Player.GetPoint(EPoint.MALL_ATT_BONUS)}% " +
                                    $"DEF:{context.Player.GetPoint(EPoint.MALL_DEF_BONUS)}% " +
                                    $"EXP:{context.Player.GetPoint(EPoint.MALL_EXP_BONUS)}% " +
                                    $"ITEMx{context.Player.GetPoint(EPoint.MALL_ITEM_BONUS) / 10} " +
                                    $"GOLDx{context.Player.GetPoint(EPoint.MALL_GOLD_BONUS) / 10}");

        context.Player.SendChatInfo("BONUS:");
        context.Player.SendChatInfo($"   SKILL:{context.Player.GetPoint(EPoint.SKILL_DAMAGE_BONUS)}% " +
                                    $"NORMAL:{context.Player.GetPoint(EPoint.NORMAL_HIT_DAMAGE_BONUS)}% " +
                                    $"SKILL_DEF:{context.Player.GetPoint(EPoint.SKILL_DEFEND_BONUS)}% " +
                                    $"NORMAL_DEF:{context.Player.GetPoint(EPoint.NORMAL_HIT_DEFEND_BONUS)}%");

        context.Player.SendChatInfo($"   HUMAN:{context.Player.GetPoint(EPoint.ATTACK_BONUS_HUMAN)}% " +
                                    $"ANIMAL:{context.Player.GetPoint(EPoint.ATTACK_BONUS_ANIMAL)}% " +
                                    $"ORC:{context.Player.GetPoint(EPoint.ATTACK_BONUS_ORC)}% " +
                                    $"ESO:{context.Player.GetPoint(EPoint.ATTACK_BONUS_ESOTERICS)}% " +
                                    $"UNDEAD:{context.Player.GetPoint(EPoint.ATTACK_BONUS_UNDEAD)}%");

        context.Player.SendChatInfo($"   DEVIL:{context.Player.GetPoint(EPoint.ATTACK_BONUS_DEVIL)}% " +
                                    $"INSECT:{context.Player.GetPoint(EPoint.ATTACK_BONUS_INSECT)}% " +
                                    $"FIRE:{context.Player.GetPoint(EPoint.ATTACK_BONUS_FIRE)}% " +
                                    $"ICE:{context.Player.GetPoint(EPoint.ATTACK_BONUS_ICE)}% " +
                                    $"DESERT:{context.Player.GetPoint(EPoint.ATTACK_BONUS_DESERT)}%");

        context.Player.SendChatInfo($"   TREE:{context.Player.GetPoint(EPoint.ATTACK_BONUS_TREE)}% " +
                                    $"MONSTER:{context.Player.GetPoint(EPoint.ATTACK_BONUS_MONSTER)}%");

        context.Player.SendChatInfo($"   WARR:{context.Player.GetPoint(EPoint.ATTACK_BONUS_WARRIOR)}% " +
                                    $"ASAS:{context.Player.GetPoint(EPoint.ATTACK_BONUS_ASSASSIN)}% " +
                                    $"SURA:{context.Player.GetPoint(EPoint.ATTACK_BONUS_SURA)}% " +
                                    $"SHAM:{context.Player.GetPoint(EPoint.ATTACK_BONUS_SHAMAN)}%");

        return Task.CompletedTask;
    }
}
