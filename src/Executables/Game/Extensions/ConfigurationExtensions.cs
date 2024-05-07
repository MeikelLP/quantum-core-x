using Microsoft.Extensions.Configuration;

namespace QuantumCore.Game.Extensions;

public static class ConfigurationExtensions
{
    public static void AddQuantumCoreDefaults(this IConfigurationBuilder config)
    {
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            {"job:0:id", "0"},
            {"job:0:name", "warrior"},
            {"job:0:st", "6"},
            {"job:0:ht", "4"},
            {"job:0:dx", "3"},
            {"job:0:iq", "3"},
            {"job:0:start_hp", "600"},
            {"job:0:start_sp", "200"},
            {"job:0:hp_per_ht", "40"},
            {"job:0:sp_per_iq", "20"},
            {"job:0:hp_per_level", "36"},
            {"job:0:sp_per_level", "44"},

            {"job:1:id", "1"},
            {"job:1:name", "assassin"},
            {"job:1:st", "4"},
            {"job:1:ht", "3"},
            {"job:1:dx", "6"},
            {"job:1:iq", "3"},
            {"job:1:start_hp", "650"},
            {"job:1:start_sp", "200"},
            {"job:1:hp_per_ht", "40"},
            {"job:1:sp_per_iq", "20"},
            {"job:1:hp_per_level", "36"},
            {"job:1:sp_per_level", "44"},

            {"job:2:id", "2"},
            {"job:2:name", "sura"},
            {"job:2:st", "5"},
            {"job:2:ht", "3"},
            {"job:2:dx", "3"},
            {"job:2:iq", "5"},
            {"job:2:start_hp", "650"},
            {"job:2:start_sp", "200"},
            {"job:2:hp_per_ht", "40"},
            {"job:2:sp_per_iq", "20"},
            {"job:2:hp_per_level", "36"},
            {"job:2:sp_per_level", "44"},

            {"job:3:id", "3"},
            {"job:3:name", "shamana"},
            {"job:3:st", "3"},
            {"job:3:ht", "4"},
            {"job:3:dx", "3"},
            {"job:3:iq", "6"},
            {"job:3:start_hp", "700"},
            {"job:3:start_sp", "200"},
            {"job:3:hp_per_ht", "40"},
            {"job:3:sp_per_iq", "20"},
            {"job:3:hp_per_level", "36"},
            {"job:3:sp_per_level", "44"},
        });
    }
}