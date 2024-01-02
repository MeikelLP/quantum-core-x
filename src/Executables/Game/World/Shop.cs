using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Packets.Shop;
using QuantumCore.Game.World.Entities;
using ShopItem = QuantumCore.API.Core.Models.ShopItem;

namespace QuantumCore.Game.World;

public class ShopDefinition
{
    public int Id { get; set; }
    public uint? Npc { get; set; }
}

public class Shop : IShop
{
    // todo: implement player shop, this require to remove an item on purchase
    //       and we also have to make sure to only execute one buy at a time
    //       to prevent an item from getting sold multiple times

    public uint Vid { get; set; }
    public string Name { get; set; } = "";
    public IReadOnlyList<ShopItem> Items { get { return _items; } }
    public List<IPlayerEntity> Visitors { get; } = new();

    private Grid<ShopItem> _grid = new(4, 5);
    private readonly List<ShopItem> _items = new();
    private readonly IItemManager _itemManager;
    private readonly IShopsManager _shopsManager;
    private readonly ILogger _logger;

    public Shop(IItemManager itemManager, IShopsManager shopsManager, ILogger logger)
    {
        _itemManager = itemManager;
        _shopsManager = shopsManager;
        _logger = logger;

        var shopItems = _shopsManager.GetShopItems(Vid);
        foreach (var item in shopItems.Result)
        {
            AddItem(item.ItemId, item.Count, item.Price);
        }
    }

    public void AddItem(uint itemId, byte count, uint price)
    {
        var proto = _itemManager.GetItem(itemId);
        if (proto == null)
        {
            return;
        }

        var (x, y) = _grid.GetFreePosition(1, proto.Size);
        if (x == -1)
        {
            return;
        }

        var position = (byte) (x + y * _grid.Width);
        var item = new ShopItem {
            ItemId = itemId, Count = count, Price = price == 0 ? proto.BuyPrice * count : price, Position = position
        };
        _items.Add(item);
        _grid.SetBlock((uint) x, (uint) y, 1, proto.Size, item);
    }

    public void Open(IPlayerEntity player)
    {
        if (player is not PlayerEntity p)
        {
            return;
        }

        p.Shop = this;
        Visitors.Add(p);

        var shopStart = new ShopOpen { Vid = Vid };
        foreach (var item in _items)
        {
            // For some reason the item also contains the position while the client uses the array index as position
            shopStart.Items[item.Position] = new Packets.Shop.ShopItem {
                Position = item.Position,
                ItemId = item.ItemId,
                Count = item.Count,
                Price = item.Price
            };
        }
        p.Connection.Send(shopStart);
    }

    public async Task Buy(IPlayerEntity player, byte position, byte count)
    {
        if (player is not PlayerEntity p)
        {
            return;
        }

        // Look up item the player wants to buy
        var item = _items.Find(item => item.Position == position);
        if (item == null)
        {
            _logger.LogInformation("{Player} tried to buy non existing item", player);
            p.Connection.Close();
            return;
        }

        var proto = _itemManager.GetItem(item.ItemId);

        if (proto is null)
        {
            _logger.LogError("Couldn't find item proto for ID {ProtoId}. This shouldn't happen", item.ItemId);
            return;
        }

        var gold = p.GetPoint(EPoints.Gold);
        if (gold < item.Price)
        {
            p.Connection.Send(new ShopNotEnoughMoney());
            return;
        }

        // Create item instance
        var playerItem = _itemManager.CreateItem(proto, item.Count);

        // todo set bonuses and sockets

        // Try to place item into players inventory
        if (!await p.Inventory.PlaceItem(playerItem))
        {
            p.Connection.Send(new ShopNoSpaceLeft());
        }
        p.AddPoint(EPoints.Gold, -(int)item.Price);

        p.SendPoints();
        p.SendItem(playerItem);
    }

    public void Sell(IPlayerEntity player, byte position)
    {
        if (player is not PlayerEntity p)
        {
            return;
        }

        var item = p.Inventory.GetItem(position);
        if (item == null)
        {
            return;
        }

        var proto = _itemManager.GetItem(item.ItemId);
        if (proto == null)
        {
            return;
        }

        if (p.DestroyItem(item))
        {
            p.AddPoint(EPoints.Gold, (int) proto.SellPrice);
            p.SendPoints();
        }
    }

    public void Close(IPlayerEntity player, bool sendClose = false)
    {
        if (player is not PlayerEntity p)
        {
            return;
        }

        p.Shop = null;
        Visitors.Remove(p);

        // todo send close if flag specified
    }
}
