using System;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Quest;

public interface IQuestManager
{
    void Init();
    void InitializePlayer(IPlayerEntity player);
    void RegisterQuest(Type questType);
}