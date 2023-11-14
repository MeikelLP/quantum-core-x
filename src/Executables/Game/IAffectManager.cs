using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Database;

namespace QuantumCore.Game;

public interface IAffectManager
{
    Task SendAffectRemovePacket(IPlayerEntity playerEntity, EAffectType type, EAffectType applyOn);
    Task AddAffect(IPlayerEntity playerEntity, EAffectType type, EAffectType applyOn, int applyValue, EAffects flags,
        int duration,
        int spCost);
    Task LoadAffect(IPlayerEntity playerEntity);
}
