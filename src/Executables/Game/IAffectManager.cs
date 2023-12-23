using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game;

public interface IAffectManager
{
    Task RemoveAffectFromPlayerAsync(IPlayerEntity playerEntity, EAffectType type, EApplyType applyOn);
    Task AddAffectToPlayerAsync(IPlayerEntity playerEntity, EAffectType type, EApplyType applyOn, int applyValue, EAffects flags,
        int duration, int spCost);
    Task LoadAffectAffectsForPlayer(IPlayerEntity playerEntity);
}
