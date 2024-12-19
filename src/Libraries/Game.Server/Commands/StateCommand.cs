using System.Text;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
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
        context.Player.SendChatInfo($"ATT {context.Player.GetPoint(EPoints.AttackGrade)} " +
                                    $"MAGIC_ATT {context.Player.GetPoint(EPoints.MagicAttackGrade)} " +
                                    $"SPD {context.Player.GetPoint(EPoints.AttackSpeed)} " +
                                    $"CRIT {context.Player.GetPoint(EPoints.CriticalPercentage)}% " +
                                    $"PENE {context.Player.GetPoint(EPoints.PenetratePercentage)}% " +
                                    $"ATT_BONUS {context.Player.GetPoint(EPoints.AttackBonus)}%");
        context.Player.SendChatInfo($"DEF {context.Player.GetPoint(EPoints.DefenceGrade)} " +
                                    $"MAGIC_DEF {context.Player.GetPoint(EPoints.MagicDefenceGrade)} " +
                                    $"BLOCK {context.Player.GetPoint(EPoints.Block)}% " +
                                    $"DODGE {context.Player.GetPoint(EPoints.Dodge)}% " +
                                    $"DEF_BONUS {context.Player.GetPoint(EPoints.DefenceBonus)}%");
        context.Player.SendChatInfo("RESISTANCES:");
        context.Player.SendChatInfo($"   WARR:{context.Player.GetPoint(EPoints.ResistWarrior)}% " +
                                    $"ASAS:{context.Player.GetPoint(EPoints.ResistAssassin)}% " +
                                    $"SURA:{context.Player.GetPoint(EPoints.ResistSura)}% " +
                                    $"SHAM:{context.Player.GetPoint(EPoints.ResistShaman)}%");
        context.Player.SendChatInfo($"   SWORD:{context.Player.GetPoint(EPoints.ResistSword)}% " +
                                    $"THSWORD:{context.Player.GetPoint(EPoints.ResistTwoHanded)}% " +
                                    $"DAGGER:{context.Player.GetPoint(EPoints.ResistDagger)}% " +
                                    $"BELL:{context.Player.GetPoint(EPoints.ResistBell)}% " +
                                    $"FAN:{context.Player.GetPoint(EPoints.ResistFan)}% " +
                                    $"BOW:{context.Player.GetPoint(EPoints.ResistBow)}%");
        context.Player.SendChatInfo($"   FIRE:{context.Player.GetPoint(EPoints.ResistFire)}% " +
                                    $"ELEC:{context.Player.GetPoint(EPoints.ResistElectric)}% " +
                                    $"MAGIC:{context.Player.GetPoint(EPoints.ResistMagic)}% " +
                                    $"WIND:{context.Player.GetPoint(EPoints.ResistWind)}% " +
                                    $"CRIT:{context.Player.GetPoint(EPoints.ResistCritical)}% " +
                                    $"PENE:{context.Player.GetPoint(EPoints.ResistPenetrate)}%");
        context.Player.SendChatInfo($"   ICE:{context.Player.GetPoint(EPoints.ResistIce)}% " +
                                    $"EARTH:{context.Player.GetPoint(EPoints.ResistEarth)}% " +
                                    $"DARK:{context.Player.GetPoint(EPoints.ResistDark)}%");

        context.Player.SendChatInfo("MALL:");
        context.Player.SendChatInfo($"   ATT:{context.Player.GetPoint(EPoints.MallAttBonus)}% " +
                                    $"DEF:{context.Player.GetPoint(EPoints.MallDefBonus)}% " +
                                    $"EXP:{context.Player.GetPoint(EPoints.MallExpBonus)}% " +
                                    $"ITEMx{context.Player.GetPoint(EPoints.MallItemBonus) / 10} " +
                                    $"GOLDx{context.Player.GetPoint(EPoints.MallGoldBonus) / 10}");

        context.Player.SendChatInfo("BONUS:");
        context.Player.SendChatInfo($"   SKILL:{context.Player.GetPoint(EPoints.SkillDamageBonus)}% " +
                                    $"NORMAL:{context.Player.GetPoint(EPoints.NormalHitDamageBonus)}% " +
                                    $"SKILL_DEF:{context.Player.GetPoint(EPoints.SkillDefendBonus)}% " +
                                    $"NORMAL_DEF:{context.Player.GetPoint(EPoints.NormalHitDefendBonus)}%");

        context.Player.SendChatInfo($"   HUMAN:{context.Player.GetPoint(EPoints.AttackBonusHuman)}% " +
                                    $"ANIMAL:{context.Player.GetPoint(EPoints.AttackBonusAnimal)}% " +
                                    $"ORC:{context.Player.GetPoint(EPoints.AttackBonusOrc)}% " +
                                    $"ESO:{context.Player.GetPoint(EPoints.AttackBonusEsoterics)}% " +
                                    $"UNDEAD:{context.Player.GetPoint(EPoints.AttackBonusUndead)}%");

        context.Player.SendChatInfo($"   DEVIL:{context.Player.GetPoint(EPoints.AttackBonusDevil)}% " +
                                    $"INSECT:{context.Player.GetPoint(EPoints.AttackBonusInsect)}% " +
                                    $"FIRE:{context.Player.GetPoint(EPoints.AttackBonusFire)}% " +
                                    $"ICE:{context.Player.GetPoint(EPoints.AttackBonusIce)}% " +
                                    $"DESERT:{context.Player.GetPoint(EPoints.AttackBonusDesert)}%");

        context.Player.SendChatInfo($"   TREE:{context.Player.GetPoint(EPoints.AttackBonusTree)}% " +
                                    $"MONSTER:{context.Player.GetPoint(EPoints.AttackBonusMonster)}%");

        context.Player.SendChatInfo($"   WARR:{context.Player.GetPoint(EPoints.AttackBonusWarrior)}% " +
                                    $"ASAS:{context.Player.GetPoint(EPoints.AttackBonusAssassin)}% " +
                                    $"SURA:{context.Player.GetPoint(EPoints.AttackBonusSura)}% " +
                                    $"SHAM:{context.Player.GetPoint(EPoints.AttackBonusShaman)}%");

        return Task.CompletedTask;
    }
}
