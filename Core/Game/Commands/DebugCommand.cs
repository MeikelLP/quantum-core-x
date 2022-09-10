using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
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

    [Command("debug_quest", "Print debug information on current quests and states")]
    public class DebugCommandQuest
    {
        [CommandMethod("List all current active quests for the current character")]
        public static void ListActiveQuests(IPlayerEntity player)
        {
            if (player is not PlayerEntity p)
            {
                return;
            }

            player.SendChatInfo("Active quests:");
            foreach (var quest in p.Quests)
            {
                player.SendChatInfo($"- {quest.Key}");
            }
        }

        [CommandMethod("Shows the current state of the given quest")]
        public static void GetQuestState(IPlayerEntity player, string questId)
        {
            if (player is not PlayerEntity p)
            {
                return;
            }

            if (!p.Quests.ContainsKey(questId))
            {
                player.SendChatInfo($"Quest {questId} is not active");
                return;
            }

            player.SendChatInfo($"{questId}:");
            var quest = p.Quests[questId];
            foreach (var name in quest.State.Keys)
            {
                player.SendChatInfo($"- {name} = {quest.State.Get(name)}");
            }
        }
    }
}