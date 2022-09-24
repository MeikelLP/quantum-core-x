using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuantumCore.API.Game.Types;
using Tomlyn;
using Tomlyn.Model;

namespace QuantumCore.Game.PlayerUtils
{
    public class JobManager : IJobManager
    {
        private readonly List<Job> _jobs = new();
        private readonly ILogger<JobManager> _logger;

        public JobManager(ILogger<JobManager> logger)
        {
            _logger = logger;
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
        
        public async Task LoadAsync(CancellationToken token = default)
        {
            _logger.LogInformation("Loading jobs.toml");
            
            var path = Path.Join("data", "jobs.toml");
            var toml = Toml.Parse(await File.ReadAllTextAsync(path, token));
            var model = toml.ToModel();
            if (model["job"] is TomlTableArray groups)
            {
                foreach (var job in groups)
                {
                    var id = (int)(job["id"] as long? ?? -1) + 1;

                    if (id == 0)
                        continue;
                        
                    // for (var i = Jobs.Count - 1; i < id; i++)
                    //     Jobs.Add(new Job());

                    var newJob = new Job {
                        Ht = (byte) (job["ht"] as long? ?? 0), 
                        Dx = (byte) (job["dx"] as long? ?? 0), 
                        St = (byte) (job["st"] as long? ?? 0),
                        Iq = (byte) (job["iq"] as long? ?? 0),
                        StartHp = (uint) (job["start_hp"] as long? ?? 0),
                        StartSp = (uint) (job["start_sp"] as long? ?? 0),
                        HpPerHt = (uint) (job["hp_per_ht"] as long? ?? 0),
                        SpPerIq = (uint) (job["sp_per_iq"] as long? ?? 0),
                        HpPerLevel = (uint) (job["hp_per_level"] as long? ?? 0),
                        SpPerLevel = (uint) (job["sp_per_level"] as long? ?? 0)
                    };
                    if (job.ContainsKey("attack_status") && job["attack_status"] is string attackStatus)
                    {
                        newJob.AttackStatus = StringToPoints(attackStatus);
                    }
                    else
                    {
                        _logger.LogError("Missing attack status in job {Name}, falling back to ST!", job["name"]);
                        newJob.AttackStatus = EPoints.St;
                    }
                    _jobs.Add(newJob);
                }
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