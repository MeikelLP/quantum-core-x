using QuantumCore.API.Game.Types.Players;

namespace QuantumCore.API;

public interface IJobManager
{
    Job? Get(EPlayerClassGendered playerClass);
}
