namespace QuantumCore.Game.World.Entities
{
    public interface IDamageable
    {
        public long TakeDamage(long damage, Entity attacker);
    }
}