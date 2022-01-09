using System.Collections.Generic;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.World
{
    public class MonsterGroup
    {
        public List<MonsterEntity> Monsters { get; } = new();
        public SpawnPoint SpawnPoint { get; set; }

        public void TriggerAll(Entity attacker, MonsterEntity except)
        {
            foreach (var monster in Monsters)
            {
                monster.TakeDamage(0, attacker);
            }
        }
    }
}