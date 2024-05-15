using Microsoft.Extensions.Configuration;

namespace QuantumCore.Game.Extensions;

public static class ConfigurationExtensions
{
    public static void AddQuantumCoreDefaults(this IConfigurationBuilder config)
    {
        // Empire Start Locations
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            {"game:empire:0:x", "0"},        // Ignored
            {"game:empire:0:y", "0"},
            {"game:empire:1:x", "475000"},   // Red
            {"game:empire:1:y", "966100"},
            {"game:empire:2:x", "60000"},    // Yellow
            {"game:empire:2:y", "156000"},
            {"game:empire:3:x", "963400"},   // Blue
            {"game:empire:3:y", "278200"},
        });
        
        // Character Stats
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            {"job:0:Id", "0"},
            {"job:0:Name", "warrior"},
            {"job:0:St", "6"},
            {"job:0:Ht", "4"},
            {"job:0:Dx", "3"},
            {"job:0:Iq", "3"},
            {"job:0:StartHp", "600"},
            {"job:0:StartSp", "200"},
            {"job:0:HpPerHt", "40"},
            {"job:0:SpPerIq", "20"},
            {"job:0:HpPerLevel", "36"},
            {"job:0:SpPerLevel", "44"},

            {"job:1:Id", "1"},
            {"job:1:Name", "assassin"},
            {"job:1:St", "4"},
            {"job:1:Ht", "3"},
            {"job:1:Dx", "6"},
            {"job:1:Iq", "3"},
            {"job:1:StartHp", "650"},
            {"job:1:StartSp", "200"},
            {"job:1:HpPerHt", "40"},
            {"job:1:SpPerIq", "20"},
            {"job:1:HpPerLevel", "36"},
            {"job:1:SpPerLevel", "44"},

            {"job:2:Id", "2"},
            {"job:2:Name", "sura"},
            {"job:2:St", "5"},
            {"job:2:Ht", "3"},
            {"job:2:Dx", "3"},
            {"job:2:Iq", "5"},
            {"job:2:StartHp", "650"},
            {"job:2:StartSp", "200"},
            {"job:2:HpPerHt", "40"},
            {"job:2:SpPerIq", "20"},
            {"job:2:HpPerLevel", "36"},
            {"job:2:SpPerLevel", "44"},

            {"job:3:Id", "3"},
            {"job:3:Name", "shamana"},
            {"job:3:St", "3"},
            {"job:3:Ht", "4"},
            {"job:3:Dx", "3"},
            {"job:3:Iq", "6"},
            {"job:3:StartHp", "700"},
            {"job:3:StartSp", "200"},
            {"job:3:HpPerHt", "40"},
            {"job:3:SpPerIq", "20"},
            {"job:3:HpPerLevel", "36"},
            {"job:3:SpPerLevel", "44"},

            {"job:4:Id", "4"},
            {"job:4:Name", "warrior"},
            {"job:4:St", "6"},
            {"job:4:Ht", "4"},
            {"job:4:Dx", "3"},
            {"job:4:Iq", "3"},
            {"job:4:StartHp", "600"},
            {"job:4:StartSp", "200"},
            {"job:4:HpPerHt", "40"},
            {"job:4:SpPerIq", "20"},
            {"job:4:HpPerLevel", "36"},
            {"job:4:SpPerLevel", "44"},

            {"job:5:Id", "5"},
            {"job:5:Name", "assassin"},
            {"job:5:St", "4"},
            {"job:5:Ht", "3"},
            {"job:5:Dx", "6"},
            {"job:5:Iq", "3"},
            {"job:5:StartHp", "650"},
            {"job:5:StartSp", "200"},
            {"job:5:HpPerHt", "40"},
            {"job:5:SpPerIq", "20"},
            {"job:5:HpPerLevel", "36"},
            {"job:5:SpPerLevel", "44"},

            {"job:6:Id", "6"},
            {"job:6:Name", "sura"},
            {"job:6:St", "5"},
            {"job:6:Ht", "3"},
            {"job:6:Dx", "3"},
            {"job:6:Iq", "5"},
            {"job:6:StartHp", "650"},
            {"job:6:StartSp", "200"},
            {"job:6:HpPerHt", "40"},
            {"job:6:SpPerIq", "20"},
            {"job:6:HpPerLevel", "36"},
            {"job:6:SpPerLevel", "44"},

            {"job:7:Id", "7"},
            {"job:7:Name", "shamana"},
            {"job:7:St", "3"},
            {"job:7:Ht", "4"},
            {"job:7:Dx", "3"},
            {"job:7:Iq", "6"},
            {"job:7:StartHp", "700"},
            {"job:7:StartSp", "200"},
            {"job:7:HpPerHt", "40"},
            {"job:7:SpPerIq", "20"},
            {"job:7:HpPerLevel", "36"},
            {"job:7:SpPerLevel", "44"},
        });
    }
}
