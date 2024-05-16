using QuantumCore.API.Game.World;

namespace QuantumCore.API;

public interface IQuestManager
{
    void Init();
    void InitializePlayer(IPlayerEntity player);
    void RegisterQuest(Type questType);
}