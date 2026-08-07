using System.Collections.Immutable;
using Csv;
using Csv.Annotations;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using QuantumCore.API;

namespace QuantumCore.Game.Shops;

[CsvObject]
internal partial class TsvShop
{
    [Column(0)] public uint ShopId { get; set; }
    [Column(2)] public uint NpcId { get; set; }
}

[CsvObject]
internal partial class TsvShopItem
{
    [Column(0)] public uint ShopId { get; set; }
    [Column(1)] public uint ItemId { get; set; }
    [Column(2)] public byte Amount { get; set; }
}

internal class TsvShopProvider : INpcShopProvider, ILoadable
{
    private readonly IFileProvider _fileProvider;
    private readonly ILogger<TsvShopProvider> _logger;
    public ImmutableArray<ShopMonsterInfo> Shops { get; private set; } = [];

    public TsvShopProvider(IFileProvider fileProvider, ILogger<TsvShopProvider> logger)
    {
        _fileProvider = fileProvider;
        _logger = logger;
    }

    public async Task LoadAsync(CancellationToken token = default)
    {
        var shopsFile = _fileProvider.GetFileInfo("shops.tsv");

        if (!shopsFile.Exists)
        {
            throw new FileNotFoundException($"{shopsFile.PhysicalPath} does not exist");
        }

        var shopItemsFile = _fileProvider.GetFileInfo("shop_items.tsv");

        if (!shopItemsFile.Exists)
        {
            throw new FileNotFoundException($"{shopItemsFile.PhysicalPath} does not exist");
        }

        await using var shopsFs = shopsFile.CreateReadStream();
        await using var shopItemsFs = shopItemsFile.CreateReadStream();
        var tsvOptions = new CsvOptions
        {
            Separator = SeparatorType.Tab,
            HasHeader = true,
            QuoteMode = QuoteMode.None,
        };
        await Task.Delay(10_000, token);
        _logger.LogInformation("Started reading tsv");
        var shops = await CsvSerializer.DeserializeAsync<TsvShop>(shopsFs, tsvOptions, cancellationToken: token);
        var shopItems =
            await CsvSerializer.DeserializeAsync<TsvShopItem>(shopItemsFs, tsvOptions, cancellationToken: token);
        Shops =
        [
            .. shops.Select(x =>
                new ShopMonsterInfo(x.NpcId,
                    [
                        .. shopItems
                            .Where(i => i.ShopId == x.ShopId)
                            .Select(i => new ShopItemInfo(i.ItemId, i.Amount))
                    ]
                )
            )
        ];
    }
}