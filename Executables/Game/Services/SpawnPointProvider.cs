using QuantumCore.Game.World;

namespace QuantumCore.Game.Services;

internal class SpawnPointProvider : ISpawnPointProvider
{
    public async Task<SpawnPoint[]> GetSpawnPointsForMap(string name)
    {
        var list = new List<SpawnPoint>();

        await AddSpawnPointsFromFile($"data/maps/{name}/regen.txt", list);
        await AddSpawnPointsFromFile($"data/maps/{name}/npc.txt", list);

        return list.ToArray();
    }

    private static async Task AddSpawnPointsFromFile(string filePath, List<SpawnPoint> list)
    {
        if (!File.Exists(filePath)) return;
        using var sr = new StreamReader(filePath);
        do
        {
            var line = await sr.ReadLineAsync();
            if (line is null) break;

            var spawn = Game.ParserUtils.GetSpawnFromLine(line);
            if (spawn is not null)
            {
                list.Add(spawn);
            }
        } while (!sr.EndOfStream);
    }
}
