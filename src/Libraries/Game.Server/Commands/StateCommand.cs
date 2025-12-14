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
        var localX = (uint)((p.PositionX - p.Map!.Position.X) / (float)Map.SpawnPositionMultiplier);
        var localY = (uint)((p.PositionY - p.Map!.Position.Y) / (float)Map.SpawnPositionMultiplier);
        sb.Append($"Coordinate {globalX}x{globalY} ({localX}x{localY}) Map {p.Map.Name}");

        context.Player.SendChatInfo(sb.ToString());
        sb.Clear();

        context.Player.SendChatInfo($"LEV {p.Player.Level}");
        context.Player.SendChatInfo($"HP {p.Health}/{p.Player.MaxHp}");
        context.Player.SendChatInfo($"SP {p.Mana}/{p.Player.MaxSp}");
        context.Player.SendChatInfo($"ATT {context.Player.GetPoint(EPoint.AttackGrade)} " +
                                    $"MAGIC_ATT {context.Player.GetPoint(EPoint.MagicAttackGrade)} " +
                                    $"SPD {context.Player.GetPoint(EPoint.AttackSpeed)} " +
                                    $"CRIT {context.Player.GetPoint(EPoint.CriticalPercentage)}% " +
                                    $"PENE {context.Player.GetPoint(EPoint.PenetratePercentage)}% " +
                                    $"ATT_BONUS {context.Player.GetPoint(EPoint.AttackBonus)}%");
        context.Player.SendChatInfo($"DEF {context.Player.GetPoint(EPoint.DefenceGrade)} " +
                                    $"MAGIC_DEF {context.Player.GetPoint(EPoint.MagicDefenceGrade)} " +
                                    $"BLOCK {context.Player.GetPoint(EPoint.Block)}% " +
                                    $"DODGE {context.Player.GetPoint(EPoint.Dodge)}% " +
                                    $"DEF_BONUS {context.Player.GetPoint(EPoint.DefenceBonus)}%");
        context.Player.SendChatInfo("RESISTANCES:");
        context.Player.SendChatInfo($"   WARR:{context.Player.GetPoint(EPoint.ResistWarrior)}% " +
                                    $"ASAS:{context.Player.GetPoint(EPoint.ResistAssassin)}% " +
                                    $"SURA:{context.Player.GetPoint(EPoint.ResistSura)}% " +
                                    $"SHAM:{context.Player.GetPoint(EPoint.ResistShaman)}%");
        context.Player.SendChatInfo($"   SWORD:{context.Player.GetPoint(EPoint.ResistSword)}% " +
                                    $"THSWORD:{context.Player.GetPoint(EPoint.ResistTwoHanded)}% " +
                                    $"DAGGER:{context.Player.GetPoint(EPoint.ResistDagger)}% " +
                                    $"BELL:{context.Player.GetPoint(EPoint.ResistBell)}% " +
                                    $"FAN:{context.Player.GetPoint(EPoint.ResistFan)}% " +
                                    $"BOW:{context.Player.GetPoint(EPoint.ResistBow)}%");
        context.Player.SendChatInfo($"   FIRE:{context.Player.GetPoint(EPoint.ResistFire)}% " +
                                    $"ELEC:{context.Player.GetPoint(EPoint.ResistElectric)}% " +
                                    $"MAGIC:{context.Player.GetPoint(EPoint.ResistMagic)}% " +
                                    $"WIND:{context.Player.GetPoint(EPoint.ResistWind)}% " +
                                    $"CRIT:{context.Player.GetPoint(EPoint.ResistCritical)}% " +
                                    $"PENE:{context.Player.GetPoint(EPoint.ResistPenetrate)}%");
        context.Player.SendChatInfo($"   ICE:{context.Player.GetPoint(EPoint.ResistIce)}% " +
                                    $"EARTH:{context.Player.GetPoint(EPoint.ResistEarth)}% " +
                                    $"DARK:{context.Player.GetPoint(EPoint.ResistDark)}%");

        context.Player.SendChatInfo("MALL:");
        context.Player.SendChatInfo($"   ATT:{context.Player.GetPoint(EPoint.MallAttBonus)}% " +
                                    $"DEF:{context.Player.GetPoint(EPoint.MallDefBonus)}% " +
                                    $"EXP:{context.Player.GetPoint(EPoint.MallExpBonus)}% " +
                                    $"ITEMx{context.Player.GetPoint(EPoint.MallItemBonus) / 10} " +
                                    $"GOLDx{context.Player.GetPoint(EPoint.MallGoldBonus) / 10}");

        context.Player.SendChatInfo("BONUS:");
        context.Player.SendChatInfo($"   SKILL:{context.Player.GetPoint(EPoint.SkillDamageBonus)}% " +
                                    $"NORMAL:{context.Player.GetPoint(EPoint.NormalHitDamageBonus)}% " +
                                    $"SKILL_DEF:{context.Player.GetPoint(EPoint.SkillDefendBonus)}% " +
                                    $"NORMAL_DEF:{context.Player.GetPoint(EPoint.NormalHitDefendBonus)}%");

        context.Player.SendChatInfo($"   HUMAN:{context.Player.GetPoint(EPoint.AttackBonusHuman)}% " +
                                    $"ANIMAL:{context.Player.GetPoint(EPoint.AttackBonusAnimal)}% " +
                                    $"ORC:{context.Player.GetPoint(EPoint.AttackBonusOrc)}% " +
                                    $"ESO:{context.Player.GetPoint(EPoint.AttackBonusEsoterics)}% " +
                                    $"UNDEAD:{context.Player.GetPoint(EPoint.AttackBonusUndead)}%");

        context.Player.SendChatInfo($"   DEVIL:{context.Player.GetPoint(EPoint.AttackBonusDevil)}% " +
                                    $"INSECT:{context.Player.GetPoint(EPoint.AttackBonusInsect)}% " +
                                    $"FIRE:{context.Player.GetPoint(EPoint.AttackBonusFire)}% " +
                                    $"ICE:{context.Player.GetPoint(EPoint.AttackBonusIce)}% " +
                                    $"DESERT:{context.Player.GetPoint(EPoint.AttackBonusDesert)}%");

        context.Player.SendChatInfo($"   TREE:{context.Player.GetPoint(EPoint.AttackBonusTree)}% " +
                                    $"MONSTER:{context.Player.GetPoint(EPoint.AttackBonusMonster)}%");

        context.Player.SendChatInfo($"   WARR:{context.Player.GetPoint(EPoint.AttackBonusWarrior)}% " +
                                    $"ASAS:{context.Player.GetPoint(EPoint.AttackBonusAssassin)}% " +
                                    $"SURA:{context.Player.GetPoint(EPoint.AttackBonusSura)}% " +
                                    $"SHAM:{context.Player.GetPoint(EPoint.AttackBonusShaman)}%");

        return Task.CompletedTask;
    }
}
