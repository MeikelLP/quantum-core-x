using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuantumCore.API.Game.Types;

namespace QuantumCore.Game.PlayerUtils
{
    public class JobManager : IJobManager
    {
        private readonly List<Job> _jobs = new();
        private readonly ILogger<JobManager> _logger;

        public JobManager(ILogger<JobManager> logger, IConfiguration configuration)
        {
            _logger = logger;

            var jobs = configuration.GetSection("job").Get<Job[]>();

            if (jobs is not null)
            {
                _jobs.AddRange(jobs);
            }
            else
            {
                _logger.LogWarning("No jobs found. This may cause issues later on");
            }
        }

        public byte GetJobFromClass(byte playerClass)
        {
            switch (playerClass)
            {
                case 0:
                case 4:
                    return 0;
                case 1:
                case 5:
                    return 1;
                case 2:
                case 6:
                    return 2;
                case 3:
                case 7:
                    return 3;
                default:
                    return 0;
            }
        }

        private EPoints StringToPoints(string str)
        {
            switch (str.ToLower())
            {
                case "ht":
                    return EPoints.Ht;
                case "st":
                    return EPoints.St;
                case "dx":
                    return EPoints.Dx;
                case "iq":
                    return EPoints.Iq;
            }

            _logger.LogError("Invalid status {Status}", str);
            return EPoints.St;
        }
        
        public Job Get(byte playerClass)
        {
            var id = GetJobFromClass(playerClass) + 1;
            if (_jobs.Count < id)
            {
                return _jobs[0];
            }

            return _jobs[id];
        }
    }
}