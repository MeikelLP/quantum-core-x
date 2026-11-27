using QuantumCore.Game.Types.Entities;
using QuantumCore.Models;

namespace QuantumCore;

public interface IAnimationManager
{
    /// <summary>
    /// Get animation for specific entity type id
    /// </summary>
    /// <param name="id">Player Class or Monster ID</param>
    /// <param name="type">The main animation type</param>
    /// <param name="subType">The sub animation type</param>
    /// <returns>The animation or null if the animation doesn't exists</returns>
    Animation? GetAnimation(uint id, AnimationType type, AnimationSubType subType);
}
