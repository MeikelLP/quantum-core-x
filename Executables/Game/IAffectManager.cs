using QuantumCore.API.Game.World;
using QuantumCore.Database;

namespace QuantumCore.Game;

public interface IAffectManager
{
    void SendAffectAddPacket(IPlayerEntity playerEntity, Affect affect, int duration);
    Task SendAffectRemovePacket(IPlayerEntity playerEntity, long type, byte applyOn);
    Task AddAffect(IPlayerEntity playerEntity, int type, int applyOn, int applyValue, int flag, int duration, int spCost);
    Task LoadAffect(IPlayerEntity playerEntity);
}
