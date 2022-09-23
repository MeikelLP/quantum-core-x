using System.Threading;
using System.Threading.Tasks;

namespace QuantumCore.Game.PlayerUtils;

public interface IJobManager
{
    byte GetJobFromClass(byte playerClass);
    Task LoadAsync(CancellationToken token = default);
    Job Get(byte playerClass);
}