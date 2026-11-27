using QuantumCore.API.Core.Timekeeping;

namespace QuantumCore.API.Game.World.AI;

public interface IBehaviour
{
    /// <summary>
    /// Initialize this behaviour instance for the given entity
    /// </summary>
    /// <param name="entity">Entity who's getting controlled by this behaviour</param>
    void Init(IEntity entity);

    /// <summary>
    /// Executes behaviour logic
    /// </summary>
    void Update(TickContext ctx);

    /// <summary>
    /// Called as soon as the entity got damage
    /// </summary>
    /// <param name="attacker">Attacker / source of the damage</param>
    /// <param name="damage">Damage dealt</param>
    void TookDamage(IEntity attacker, uint damage);

    /// <summary>
    /// Called as soon as a entity enters the view of the controlled entity
    /// </summary>
    /// <param name="entity"></param>
    void OnNewNearbyEntity(IEntity entity);
}
