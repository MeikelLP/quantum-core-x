using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using QuantumCore.API;

namespace QuantumCore.Game.Shops;

internal class JsonNpcShopProvider : INpcShopProvider, ILoadable
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
    };

    private readonly IFileProvider _fileProvider;
    private readonly ILogger<JsonNpcShopProvider> _logger;
    public ImmutableArray<ShopMonsterInfo> Shops { get; private set; }

    public JsonNpcShopProvider(IFileProvider fileProvider, ILogger<JsonNpcShopProvider> logger)
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
            Shops = [];
            return;
        }

        await using var fs = file.CreateReadStream();
        Shops =
        [
            .. await JsonSerializer.DeserializeAsync<ShopMonsterInfo[]>(fs, _jsonSerializerOptions,
                cancellationToken: token) ?? []
        ];
    }
}