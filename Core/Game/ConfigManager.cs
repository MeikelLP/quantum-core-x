using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;
using Tomlyn;
using Tomlyn.Model;

namespace QuantumCore.Game;

public static class ConfigManager
{
    private static List<string> _maps = new();

    public static IReadOnlyList<string> Maps {
        get {
            return _maps;
        }
    }

    public static void Load()
    {
        if (!File.Exists("settings.toml"))
        {
            Log.Error("No settings.toml found!");
            Environment.Exit(1);
            return;
        }

        var str = File.ReadAllText("settings.toml");
        var toml = Toml.Parse(str);
        var model = toml.ToModel();

        if (model["maps"] is TomlArray maps)
        { 
            _maps.AddRange(maps.Select(m => m as string));
        }
    }
}