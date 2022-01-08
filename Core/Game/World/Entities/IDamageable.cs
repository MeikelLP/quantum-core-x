namespace QuantumCore.Game.World.Entities
{
    public interface IDamageable
    {
        public uint GetDefence();
        public long TakeDamage(long damage, Entity attacker);
    }
}