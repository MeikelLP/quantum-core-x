using AwesomeAssertions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using QuantumCore.API;
using QuantumCore.Game.Shops;

namespace Game.Tests;

public class TsvShopProviderTests
{
    private readonly IFileProvider _fileProvider = Substitute.For<IFileProvider>();
    private readonly TsvShopProvider _provider;

    public TsvShopProviderTests()
    {
        _provider = new TsvShopProvider(_fileProvider, NullLogger<TsvShopProvider>.Instance);
    }

    [Fact]
    public async Task Load()
    {
        _fileProvider.GetFileInfo("shops.tsv").Returns(_ =>
        {
            var fileInfo = Substitute.For<IFileInfo>();
            fileInfo.Exists.Returns(true);
            fileInfo.CreateReadStream().Returns(new MemoryStream([
                .. """
                   vnum	name	npc_vnum
                   1	Weapon Shop Dealer	9001
                   """u8
            ]));
            return fileInfo;
        });
        _fileProvider.GetFileInfo("shop_items.tsv").Returns(_ =>
        {
            var fileInfo = Substitute.For<IFileInfo>();
            fileInfo.Exists.Returns(true);
            fileInfo.CreateReadStream().Returns(new MemoryStream([
                .. """
                   shop_vnum	item_vnum	count
                   1	20	1
                   """u8
            ]));
            return fileInfo;
        });

        await _provider.LoadAsync(TestContext.Current.CancellationToken);
        _provider.Shops.Should().BeEquivalentTo([
            new ShopMonsterInfo(9001, [
                new ShopItemInfo(20)
            ])
        ]);
    }
}