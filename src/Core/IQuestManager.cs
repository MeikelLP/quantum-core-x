using QuantumCore.API.Game.World;

namespace QuantumCore.API;

public interface IQuestManager
{
    void InitializePlayer(IPlayerEntity player);
    void RegisterQuest(Type questType);
}