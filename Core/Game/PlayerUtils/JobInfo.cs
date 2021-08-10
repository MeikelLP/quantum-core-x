using System.Collections.Generic;
using System.IO;
using QuantumCore.Database;
using QuantumCore.Game.World;
using Tomlyn;
using Tomlyn.Model;

namespace QuantumCore.Game.PlayerUtils
{
    public class Job
    {
        public byte Ht { get; set; }
        public byte St { get; set; }
        public byte Dx { get; set; }
        public byte Iq { get; set; }
        public uint StartHp { get; set; }
        public uint StartSp { get; set; }
        public uint HpPerHt { get; set; }
        public uint SpPerIq { get; set; }
        public uint HpPerLevel { get; set; }
        public uint SpPerLevel { get; set; }
    }

    public static class JobInfo
    {
        private static IList<Job> Jobs = new List<Job>();

        public static byte GetJobFromClass(byte playerClass)
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
        
        public static void Load()
        {
            var path = Path.Join("data", "jobs.toml");
            var toml = Toml.Parse(File.ReadAllText(path));
            var model = toml.ToModel();
            if (model["job"] is TomlTableArray groups)
            {
                foreach (var job in groups)
                {
                    var id = (int)(job["id"] as long? ?? -1) + 1;

                    if (id == 0)
                        continue;
                        
                    for (var i = Jobs.Count - 1; i < id; i++)
                        Jobs.Add(new Job());

                    Jobs[id].Ht = (byte) (job["ht"] as long? ?? 0);
                    Jobs[id].Dx = (byte) (job["dx"] as long? ?? 0);
                    Jobs[id].St = (byte) (job["st"] as long? ?? 0);
                    Jobs[id].Iq = (byte) (job["iq"] as long? ?? 0);
                    Jobs[id].StartHp = (uint) (job["start_hp"] as long? ?? 0);
                    Jobs[id].StartSp = (uint) (job["start_sp"] as long? ?? 0);
                    Jobs[id].HpPerHt = (uint) (job["hp_per_ht"] as long? ?? 0);
                    Jobs[id].SpPerIq = (uint) (job["sp_per_iq"] as long? ?? 0);
                    Jobs[id].HpPerLevel = (uint) (job["hp_per_level"] as long? ?? 0);
                    Jobs[id].SpPerLevel = (uint) (job["sp_per_level"] as long? ?? 0);
                }
            }
        }
        
        public static Job Get(byte playerClass)
        {
            var id = GetJobFromClass(playerClass) + 1;
            if (Jobs.Count < id)
            {
                return Jobs[0];
            }

            return Jobs[id];
        }
    }
}