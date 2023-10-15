using QuantumCore.Game.World;

namespace QuantumCore.Game.Services;

internal class SpawnPointProvider : ISpawnPointProvider
{
    public async Task<SpawnPoint[]> GetSpawnPointsForMap(string name)
    {
        using var sr = new StreamReader("data/maps/{name}/regen.txt");

        var list = new List<SpawnPoint>();
        do
        {
            var line = await sr.ReadLineAsync();
            if (line is null) break;

            var spawn = Game.ParserUtils.GetSpawnFromLine(line);
            list.Add(spawn);
        } while (!sr.EndOfStream);

        return list.ToArray();
    }
}
