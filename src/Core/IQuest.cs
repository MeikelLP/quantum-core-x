using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;

namespace QuantumCore.API;

public interface IQuest
{
    QuestState State { get; }
    IPlayerEntity Player { get; }
    void Init();
    void Answer(byte answer);
}