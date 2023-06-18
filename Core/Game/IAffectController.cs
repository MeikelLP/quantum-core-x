using System.Threading.Tasks;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Database;

namespace QuantumCore.Game;
public interface IAffectController
{

    void SendAffectAddPacket(IPlayerEntity playerEntity, Affect affect, int duration);
    void SendAffectRemovePacket(IPlayerEntity playerEntity, Affect affect);
    Task AddAffect(IPlayerEntity playerEntity, int type, int applyOn, int applyValue, int flag, int duration, int spCost);
    bool RemoveAffect();
}
