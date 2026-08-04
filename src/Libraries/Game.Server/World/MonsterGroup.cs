using QuantumCore.API.Game.World;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.World;

public class MonsterGroup
{
#pragma warning disable CA1002 // no lists - is okay because we wanna pool this object
    public List<MonsterEntity> Monsters { get; } = [];
#pragma warning restore CA1002
    public SpawnPoint? SpawnPoint { get; set; }

    public void TriggerAll(IEntity attacker, MonsterEntity except)
    {
        foreach (var monster in Monsters)
        {
            monster.Trigger(attacker);
        }
    }
}