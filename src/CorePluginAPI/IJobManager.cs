namespace QuantumCore.API;

public interface IJobManager
{
    byte GetJobFromClass(byte playerClass);
    Job? Get(byte playerClass);
}