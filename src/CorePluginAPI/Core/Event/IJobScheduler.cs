namespace QuantumCore.API.Core.Event;

public interface IJobScheduler
{
    void Schedule(Func<Task> work);
}
