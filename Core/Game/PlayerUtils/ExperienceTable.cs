using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Serilog;

namespace QuantumCore.Game.PlayerUtils;

public static class ExperienceTable
{
    private static readonly List<uint> _experienceTable = new();

    public static byte MaxLevel {
        get {
            return (byte) _experienceTable.Count;
        }
    }

    public static uint GetNeededExperience(byte level)
    {
        Debug.Assert(level > 0);
        return level > MaxLevel ? 0 : _experienceTable[level - 1];
    }

    public static void Load()
    {
        var path = Path.Join("data", "exp.csv");
        if (!File.Exists(path))
        {
            Log.Error("No experience table found!");
            return;
        }

        var lines = File.ReadAllLines(path);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                break;
            }

            if (!uint.TryParse(line, out var experience))
            {
                Log.Error("Failed to parse experience table!");
                _experienceTable.Clear();
                return;
            }
            
            _experienceTable.Add(experience);
        }
    }
}