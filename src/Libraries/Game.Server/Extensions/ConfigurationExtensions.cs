using Microsoft.Extensions.Configuration;

namespace QuantumCore.Game.Extensions;

public static class ConfigurationExtensions
{
    public static IConfigurationBuilder AddQuantumCoreDefaults(this IConfigurationBuilder config)
    {
        // Empire Start Locations
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            {"game:empire:0:x", "0"}, // Ignored
            {"game:empire:0:y", "0"},
            {"game:empire:1:x", "475000"}, // Red
            {"game:empire:1:y", "966100"},
            {"game:empire:2:x", "60000"}, // Yellow
            {"game:empire:2:y", "156000"},
            {"game:empire:3:x", "963400"}, // Blue
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

        // Item drops delta values
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            {"game:drops:delta:boss:0", "1"}, // -15  0
            {"game:drops:delta:boss:1", "3"}, // -14  1
            {"game:drops:delta:boss:2", "5"}, // -13  2
            {"game:drops:delta:boss:3", "7"}, // -12  3
            {"game:drops:delta:boss:4", "15"}, // -11  4
            {"game:drops:delta:boss:5", "30"}, // -10  5
            {"game:drops:delta:boss:6", "60"}, // -9   6
            {"game:drops:delta:boss:7", "90"}, // -8   7
            {"game:drops:delta:boss:8", "91"}, // -7   8
            {"game:drops:delta:boss:9", "92"}, // -6   9
            {"game:drops:delta:boss:10", "93"}, // -5   10
            {"game:drops:delta:boss:11", "94"}, // -4   11
            {"game:drops:delta:boss:12", "95"}, // -3   12
            {"game:drops:delta:boss:13", "97"}, // -2   13
            {"game:drops:delta:boss:14", "99"}, // -1   14
            {"game:drops:delta:boss:15", "100"}, // 0    15
            {"game:drops:delta:boss:16", "105"}, // 1    16
            {"game:drops:delta:boss:17", "110"}, // 2    17
            {"game:drops:delta:boss:18", "115"}, // 3    18
            {"game:drops:delta:boss:19", "120"}, // 4    19
            {"game:drops:delta:boss:20", "125"}, // 5    20
            {"game:drops:delta:boss:21", "130"}, // 6    21
            {"game:drops:delta:boss:22", "135"}, // 7    22
            {"game:drops:delta:boss:23", "140"}, // 8    23
            {"game:drops:delta:boss:24", "145"}, // 9    24
            {"game:drops:delta:boss:25", "150"}, // 10   25
            {"game:drops:delta:boss:26", "155"}, // 11   26
            {"game:drops:delta:boss:27", "160"}, // 12   27
            {"game:drops:delta:boss:28", "165"}, // 13   28
            {"game:drops:delta:boss:29", "170"}, // 14   29
            {"game:drops:delta:boss:30", "180"}, // 15   30

            {"game:drops:delta:normal:0", "1"}, // -15  0
            {"game:drops:delta:normal:1", "5"}, // -14  1
            {"game:drops:delta:normal:2", "10"}, // -13  2
            {"game:drops:delta:normal:3", "20"}, // -12  3
            {"game:drops:delta:normal:4", "30"}, // -11  4
            {"game:drops:delta:normal:5", "50"}, // -10  5
            {"game:drops:delta:normal:6", "70"}, // -9   6
            {"game:drops:delta:normal:7", "80"}, // -8   7
            {"game:drops:delta:normal:8", "85"}, // -7   8
            {"game:drops:delta:normal:9", "90"}, // -6   9
            {"game:drops:delta:normal:10", "92"}, // -5   10
            {"game:drops:delta:normal:11", "94"}, // -4   11
            {"game:drops:delta:normal:12", "96"}, // -3   12
            {"game:drops:delta:normal:13", "98"}, // -2   13
            {"game:drops:delta:normal:14", "100"}, // -1   14
            {"game:drops:delta:normal:15", "100"}, // 0    15
            {"game:drops:delta:normal:16", "105"}, // 1    16
            {"game:drops:delta:normal:17", "110"}, // 2    17
            {"game:drops:delta:normal:18", "115"}, // 3    18
            {"game:drops:delta:normal:19", "120"}, // 4    19
            {"game:drops:delta:normal:20", "125"}, // 5    20
            {"game:drops:delta:normal:21", "130"}, // 6    21
            {"game:drops:delta:normal:22", "135"}, // 7    22
            {"game:drops:delta:normal:23", "140"}, // 8    23
            {"game:drops:delta:normal:24", "145"}, // 9    24
            {"game:drops:delta:normal:25", "150"}, // 10   25
            {"game:drops:delta:normal:26", "155"}, // 11   26
            {"game:drops:delta:normal:27", "160"}, // 12   27
            {"game:drops:delta:normal:28", "165"}, // 13   28
            {"game:drops:delta:normal:29", "170"}, // 14   29
            {"game:drops:delta:normal:30", "180"}, // 15   30
        });


        config.AddInMemoryCollection(new Dictionary<string, string?>()
        {
            // metin of battle
            {"game:drops:metinstones:0:monsterprotoid", "8005"},
            {"game:drops:metinstones:0:dropchance", "60"},
            {"game:drops:metinstones:0:rankchance:0", "30"},
            {"game:drops:metinstones:0:rankchance:1", "30"},
            {"game:drops:metinstones:0:rankchance:2", "30"},
            {"game:drops:metinstones:0:rankchance:3", "9"},
            {"game:drops:metinstones:0:rankchance:4", "1"},
            // metin of greed
            {"game:drops:metinstones:1:monsterprotoid", "8006"},
            {"game:drops:metinstones:1:dropchance", "60"},
            {"game:drops:metinstones:1:rankchance:0", "28"},
            {"game:drops:metinstones:1:rankchance:1", "29"},
            {"game:drops:metinstones:1:rankchance:2", "31"},
            {"game:drops:metinstones:1:rankchance:3", "11"},
            {"game:drops:metinstones:1:rankchance:4", "1"},
            // metin of black
            {"game:drops:metinstones:2:monsterprotoid", "8007"},
            {"game:drops:metinstones:2:dropchance", "60"},
            {"game:drops:metinstones:2:rankchance:0", "24"},
            {"game:drops:metinstones:2:rankchance:1", "29"},
            {"game:drops:metinstones:2:rankchance:2", "32"},
            {"game:drops:metinstones:2:rankchance:3", "13"},
            {"game:drops:metinstones:2:rankchance:4", "2"},
            // metin of darkness
            {"game:drops:metinstones:3:monsterprotoid", "8008"},
            {"game:drops:metinstones:3:dropchance", "60"},
            {"game:drops:metinstones:3:rankchance:0", "22"},
            {"game:drops:metinstones:3:rankchance:1", "28"},
            {"game:drops:metinstones:3:rankchance:2", "33"},
            {"game:drops:metinstones:3:rankchance:3", "15"},
            {"game:drops:metinstones:3:rankchance:4", "2"},
            // metin of jealousy
            {"game:drops:metinstones:4:monsterprotoid", "8009"},
            {"game:drops:metinstones:4:dropchance", "60"},
            {"game:drops:metinstones:4:rankchance:0", "21"},
            {"game:drops:metinstones:4:rankchance:1", "27"},
            {"game:drops:metinstones:4:rankchance:2", "33"},
            {"game:drops:metinstones:4:rankchance:3", "17"},
            {"game:drops:metinstones:4:rankchance:4", "2"},
            // metin of soul
            {"game:drops:metinstones:5:monsterprotoid", "8010"},
            {"game:drops:metinstones:5:dropchance", "60"},
            {"game:drops:metinstones:5:rankchance:0", "18"},
            {"game:drops:metinstones:5:rankchance:1", "26"},
            {"game:drops:metinstones:5:rankchance:2", "34"},
            {"game:drops:metinstones:5:rankchance:3", "20"},
            {"game:drops:metinstones:5:rankchance:4", "2"},
            // metin of shadow
            {"game:drops:metinstones:6:monsterprotoid", "8011"},
            {"game:drops:metinstones:6:dropchance", "60"},
            {"game:drops:metinstones:6:rankchance:0", "14"},
            {"game:drops:metinstones:6:rankchance:1", "26"},
            {"game:drops:metinstones:6:rankchance:2", "35"},
            {"game:drops:metinstones:6:rankchance:3", "22"},
            {"game:drops:metinstones:6:rankchance:4", "3"},
            // metin of toughness
            {"game:drops:metinstones:7:monsterprotoid", "8012"},
            {"game:drops:metinstones:7:dropchance", "60"},
            {"game:drops:metinstones:7:rankchance:0", "10"},
            {"game:drops:metinstones:7:rankchance:1", "26"},
            {"game:drops:metinstones:7:rankchance:2", "37"},
            {"game:drops:metinstones:7:rankchance:3", "24"},
            {"game:drops:metinstones:7:rankchance:4", "3"},
            // metin of devil
            {"game:drops:metinstones:8:monsterprotoid", "8013"},
            {"game:drops:metinstones:8:dropchance", "60"},
            {"game:drops:metinstones:8:rankchance:0", "2"},
            {"game:drops:metinstones:8:rankchance:1", "26"},
            {"game:drops:metinstones:8:rankchance:2", "40"},
            {"game:drops:metinstones:8:rankchance:3", "29"},
            {"game:drops:metinstones:8:rankchance:4", "3"},
            // metin of fall
            {"game:drops:metinstones:9:monsterprotoid", "8014"},
            {"game:drops:metinstones:9:dropchance", "60"},
            {"game:drops:metinstones:9:rankchance:0", "0"},
            {"game:drops:metinstones:9:rankchance:1", "26"},
            {"game:drops:metinstones:9:rankchance:2", "41"},
            {"game:drops:metinstones:9:rankchance:3", "30"},
            {"game:drops:metinstones:9:rankchance:4", "3"},
        });

        // Spirit Stone Drops
        config.AddInMemoryCollection(new Dictionary<string, string?>()
        {
            {"game:drops:spiritstones:0", "28030"},
            {"game:drops:spiritstones:1", "28031"},
            {"game:drops:spiritstones:2", "28032"},
            {"game:drops:spiritstones:3", "28033"},
            {"game:drops:spiritstones:4", "28034"},
            {"game:drops:spiritstones:5", "28035"},
            {"game:drops:spiritstones:6", "28036"},
            {"game:drops:spiritstones:7", "28037"},
            {"game:drops:spiritstones:8", "28038"},
            {"game:drops:spiritstones:9", "28039"},
            {"game:drops:spiritstones:10", "28040"},
            {"game:drops:spiritstones:11", "28041"},
            {"game:drops:spiritstones:12", "28042"},
            {"game:drops:spiritstones:13", "28043"},
        });

        return config;
    }
}
