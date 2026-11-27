using QuantumCore.Game.Types.Players;

namespace QuantumCore;

public interface IJobManager
{
    Job? Get(EPlayerClassGendered playerClass);
}
