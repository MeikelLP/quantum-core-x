using QuantumCore.API.Core.Event;

namespace QuantumCore.Core.Event;

public sealed class ThreadPoolScheduler : IJobScheduler
{
    public void Schedule(Func<Task> work)
    {
        Task.Run(work);
    }
}
