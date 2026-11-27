using QuantumCore.Game.World;

namespace QuantumCore;

public interface IQuestManager
{
    void InitializePlayer(IPlayerEntity player);
    void RegisterQuest(Type questType);
}