using System.Collections.Generic;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Packets.Shop;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.World.Entities;
using Serilog;

namespace QuantumCore.Game.World;

public class Shop
{
    // todo: implement player shop, this require to remove an item on purchase
    //       and we also have to make sure to only execute one buy at a time
    //       to prevent an item from getting sold multiple times
    
    public class ShopItem
    {
        public uint ItemId { get; set; }
        public byte Count { get; set; }
        public uint Price { get; set; }
        public byte Position { get; set; }
    }

    public uint Vid { get; set; }
    public string Name { get; set; }
    public IReadOnlyList<ShopItem> Items { get { return _items; } }
    public List<PlayerEntity> Visitors { get; } = new();

    private Grid<ShopItem> _grid = new(4, 5);
    private readonly List<ShopItem> _items = new();
    
    public void AddItem(uint itemId, byte count, uint price)
    {
        var proto = ItemManager.GetItem(itemId);
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

    public async void Buy(IPlayerEntity player, byte position, byte count)
    {
        if (player is not PlayerEntity p)
        {
            return;
        }
        
        // Look up item the player wants to buy
        var item = _items.Find(item => item.Position == position);
        if (item == null)
        {
            Log.Information($"{player} tried to buy non existing item");
            p.Connection.Close();
            return;
        }

        var proto = ItemManager.GetItem(item.ItemId);

        var gold = p.GetPoint(EPoints.Gold);
        if (gold < item.Price)
        {
            p.Connection.Send(new ShopNotEnoughMoney());
            return;
        }

        // Create item instance
        var playerItem = ItemManager.CreateItem(proto, item.Count);
        
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

    public async void Sell(IPlayerEntity player, byte position)
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

        var proto = ItemManager.GetItem(item.ItemId);
        if (proto == null)
        {
            return;
        }

        if (await p.DestroyItem(item))
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