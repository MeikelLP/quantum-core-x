namespace QuantumCore.Game.PlayerUtils;

public interface IJobManager
{
    byte GetJobFromClass(byte playerClass);
    Job? Get(byte playerClass);
}
