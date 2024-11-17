using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using QuantumCore.API;

namespace QuantumCore.Game.Shops;

internal class NpcShopProvider : INpcShopProvider, ILoadable
{
    private readonly IFileProvider _fileProvider;
    private readonly ILogger<NpcShopProvider> _logger;
    public ImmutableArray<ShopMonsterInfo> Shops { get; private set; }

    public NpcShopProvider(IFileProvider fileProvider, ILogger<NpcShopProvider> logger)
    {
        _fileProvider = fileProvider;
        _logger = logger;
    }

    public async Task LoadAsync(CancellationToken token = default)
    {
        var file = _fileProvider.GetFileInfo("shops.json");

        if (!file.Exists)
        {
            _logger.LogWarning("{Path} does not exist, shops not loaded", file.PhysicalPath);
            return;
        }

        await using var fs = file.CreateReadStream();
        Shops =
        [
            ..await JsonSerializer.DeserializeAsync<ShopMonsterInfo[]>(fs, new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
            }, cancellationToken: token) ?? []
        ];
    }
}
