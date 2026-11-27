using QuantumCore.Game.World;
using QuantumCore.Models;

namespace QuantumCore;

public interface IQuest
{
    QuestState State { get; }
    IPlayerEntity Player { get; }
    void Init();
    void Answer(byte answer);
}