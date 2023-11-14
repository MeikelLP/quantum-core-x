using System.Collections.Immutable;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;

namespace QuantumCore.API;

public interface IAffectRepository
{
    Task RemoveAffectFromPlayerAsync(Guid playerId, EAffectType type, EApplyType applyOn);
    Task AddAffectAsync(Affect affect);
    Task<ImmutableArray<Affect>> GetAffectsForPlayerAsync(Guid playerId);
}
