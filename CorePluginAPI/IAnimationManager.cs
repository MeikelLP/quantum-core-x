using System.Threading;
using System.Threading.Tasks;
using QuantumCore.API.Core.Models;

namespace QuantumCore.API;

public interface IAnimationManager
{
    /// <summary>
    /// Load all the animation data for all characters and monsters.
    /// So the server can calculate movement duration.
    /// </summary>
    Task LoadAsync(CancellationToken token = default);

    /// <summary>
    /// Get animation for specific entity type id
    /// </summary>
    /// <param name="id">Player Class or Monster ID</param>
    /// <param name="type">The main animation type</param>
    /// <param name="subType">The sub animation type</param>
    /// <returns>The animation or null if the animation doesn't exists</returns>
    Animation GetAnimation(uint id, AnimationType type, AnimationSubType subType);
}