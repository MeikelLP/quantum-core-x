using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Types.Players;

namespace QuantumCore.Game.PlayerUtils;

public class JobManager : IJobManager
{
    private readonly List<Job> _jobs = new();

    public JobManager(ILogger<JobManager> logger, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var jobs = configuration.GetSection("job").Get<Job[]>();

        if (jobs is not null)
        {
            _jobs.AddRange(jobs);
        }
        else
        {
            logger.LogWarning("No jobs found. This may cause issues later on");
        }
    }

    public Job Get(EPlayerClassGendered playerClass)
    {
        if ((int)playerClass > _jobs.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(playerClass),
                $"Player class with identifier {playerClass} (index) was not found");
        }

        return _jobs[(int)playerClass];
    }
}