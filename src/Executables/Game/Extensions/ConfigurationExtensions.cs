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
        
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            {"drops:delta:boss:0", "1"},        // -15  0
            {"drops:delta:boss:1", "3"},        // -14  1
            {"drops:delta:boss:2", "5"},        // -13  2
            {"drops:delta:boss:3", "7"},        // -12  3
            {"drops:delta:boss:4", "15"},       // -11  4
            {"drops:delta:boss:5", "30"},       // -10  5
            {"drops:delta:boss:6", "60"},       // -9   6
            {"drops:delta:boss:7", "90"},       // -8   7
            {"drops:delta:boss:8", "91"},       // -7   8
            {"drops:delta:boss:9", "92"},       // -6   9
            {"drops:delta:boss:10", "93"},      // -5   10
            {"drops:delta:boss:11", "94"},      // -4   11
            {"drops:delta:boss:12", "95"},      // -3   12
            {"drops:delta:boss:13", "97"},      // -2   13
            {"drops:delta:boss:14", "99"},      // -1   14
            {"drops:delta:boss:15", "100"},     // 0    15
            {"drops:delta:boss:16", "105"},     // 1    16
            {"drops:delta:boss:17", "110"},     // 2    17
            {"drops:delta:boss:18", "115"},     // 3    18
            {"drops:delta:boss:19", "120"},     // 4    19
            {"drops:delta:boss:20", "125"},     // 5    20
            {"drops:delta:boss:21", "130"},     // 6    21
            {"drops:delta:boss:22", "135"},     // 7    22
            {"drops:delta:boss:23", "140"},     // 8    23
            {"drops:delta:boss:24", "145"},     // 9    24
            {"drops:delta:boss:25", "150"},     // 10   25
            {"drops:delta:boss:26", "155"},     // 11   26
            {"drops:delta:boss:27", "160"},     // 12   27
            {"drops:delta:boss:28", "165"},     // 13   28
            {"drops:delta:boss:29", "170"},     // 14   29
            {"drops:delta:boss:30", "180"},     // 15   30
            
            {"drops:delta:normal:0", "1"},      // -15  0
            {"drops:delta:normal:1", "5"},      // -14  1
            {"drops:delta:normal:2", "10"},     // -13  2
            {"drops:delta:normal:3", "20"},     // -12  3
            {"drops:delta:normal:4", "30"},     // -11  4
            {"drops:delta:normal:5", "50"},     // -10  5
            {"drops:delta:normal:6", "70"},     // -9   6
            {"drops:delta:normal:7", "80"},     // -8   7
            {"drops:delta:normal:8", "85"},     // -7   8
            {"drops:delta:normal:9", "90"},     // -6   9
            {"drops:delta:normal:10", "92"},    // -5   10
            {"drops:delta:normal:11", "94"},    // -4   11
            {"drops:delta:normal:12", "96"},    // -3   12
            {"drops:delta:normal:13", "98"},    // -2   13
            {"drops:delta:normal:14", "100"},   // -1   14
            {"drops:delta:normal:15", "100"},   // 0    15
            {"drops:delta:normal:16", "105"},   // 1    16
            {"drops:delta:normal:17", "110"},   // 2    17
            {"drops:delta:normal:18", "115"},   // 3    18
            {"drops:delta:normal:19", "120"},   // 4    19
            {"drops:delta:normal:20", "125"},   // 5    20
            {"drops:delta:normal:21", "130"},   // 6    21
            {"drops:delta:normal:22", "135"},   // 7    22
            {"drops:delta:normal:23", "140"},   // 8    23
            {"drops:delta:normal:24", "145"},   // 9    24
            {"drops:delta:normal:25", "150"},   // 10   25
            {"drops:delta:normal:26", "155"},   // 11   26
            {"drops:delta:normal:27", "160"},   // 12   27
            {"drops:delta:normal:28", "165"},   // 13   28
            {"drops:delta:normal:29", "170"},   // 14   29
            {"drops:delta:normal:30", "180"},   // 15   30
        });
    }
}
