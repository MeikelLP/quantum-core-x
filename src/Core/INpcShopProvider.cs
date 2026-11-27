using System.Collections.Immutable;

namespace QuantumCore.API;

public interface INpcShopProvider
{
    ImmutableArray<ShopMonsterInfo> Shops { get; }
}

public record ShopMonsterInfo(uint Monster, ImmutableArray<ShopItemInfo> Items);

public record ShopItemInfo(uint Item, byte Amount = 1);
