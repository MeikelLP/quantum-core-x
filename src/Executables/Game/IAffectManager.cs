using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game;

public interface IAffectManager
{
    Task SendAffectRemovePacket(IPlayerEntity playerEntity, EAffectType type, EApplyType applyOn);
    Task AddAffect(IPlayerEntity playerEntity, EAffectType type, EApplyType applyOn, int applyValue, EAffects flags,
        int duration,
        int spCost);
    Task LoadAffect(IPlayerEntity playerEntity);
}
