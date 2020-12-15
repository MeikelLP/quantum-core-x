using System.Security.Cryptography;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands
{
    [Command("spawn", "Spawn a monster or npc")]
    public static class SpawnCommand
    {
        [CommandMethod]
        public static async void SpawnMonster(IPlayerEntity player, uint monsterId, byte count = 1)
        {
            for (var i = 0; i < count; i++)
            {
                // Calculate random spawn position close by the player
                var x = player.PositionX + RandomNumberGenerator.GetInt32(-1500, 1501);
                var y = player.PositionY + RandomNumberGenerator.GetInt32(-1500, 1501);

                // Create entity instance
                var monster = new MonsterEntity(monsterId, x, y);
                World.World.Instance.SpawnEntity(monster);
            }
        }
    }
}