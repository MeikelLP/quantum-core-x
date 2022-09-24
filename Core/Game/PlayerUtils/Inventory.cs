using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using QuantumCore.Core.Utils;
using QuantumCore.Database;
using Serilog;

namespace QuantumCore.Game.PlayerUtils
{
    public enum WindowType
    {
        Inventory = 1
    }
    
    public class Inventory
    {
        private class Page
        {
            private readonly IItemManager _itemManager;
            private ushort _width;
            private ushort _height;

            private readonly Grid<Item> _grid;

            public Page(IItemManager itemManager, ushort width, ushort height)
            {
                _itemManager = itemManager;
                _width = width;
                _height = height;
                
                _grid = new Grid<Item>(_width, _height);
            }

            public Item GetItem(long position)
            {
                if (position < 0) return null;
                if (position >= _width * _height) return null;
                
                var x = (uint)(position % _width);
                var y = (uint)(position / _width);

                return _grid.Get(x, y);
            }

            public bool RemoveItem(long position)
            {
                if (position < 0) return false;
                if (position >= _width * _height) return false;
                
                var x = (uint)(position % _width);
                var y = (uint)(position / _width);

                var item = _grid.Get(x, y);
                if (item == null) return false;
                
                var proto = _itemManager.GetItem(item.ItemId);
                if (proto == null) return false;

                var itemSize = proto.Size;
                for (byte i = 0; i < itemSize; i++)
                {
                    _grid.Set(x, y + i, null);
                }

                return true;
            }

            /// <summary>
            /// Tries to place the given item in the inventory page
            /// </summary>
            /// <param name="item">Item to place</param>
            /// <returns>Position it got asserted or -1 if no space was found</returns>
            public long Place(Item item)
            {
                for (uint y = 0; y < _height; y++)
                {
                    for (uint x = 0; x < _width; x++)
                    {
                        if (Place(item, x, y)) return x + y * _width;
                    }    
                }

                return -1;
            }

            public bool Place(Item item, uint x, uint y)
            {
                var proto = _itemManager.GetItem(item.ItemId);
                if (proto == null) return false;
                var itemSize = proto.Size;

                // Check if all required positions are free and in bounds
                if (!IsSpaceAvailable(x, y, proto.Size)) return false;

                // Place the item
                for (byte i = 0; i < itemSize; i++)
                {
                    _grid.Set(x, y + i, item);
                }
                
                return true;
            }

            public bool Place(Item item, long position)
            {
                if (position < 0) return false;
                if (position >= _width * _height) return false;

                var x = (uint)(position % _width);
                var y = (uint)(position / _width);

                return Place(item, x, y);
            }

            public bool IsSpaceAvailable(Item item, long position)
            {
                if (position < 0) return false;
                if (position >= _width * _height) return false;
                
                var x = (uint)(position % _width);
                var y = (uint)(position / _width);

                var proto = _itemManager.GetItem(item.ItemId);
                if (proto == null) return false;

                return IsSpaceAvailable(x, y, proto.Size);
            }

            public bool IsSpaceAvailable(uint x, uint y, byte size)
            {
                for (byte i = 0; i < size; i++)
                {
                    if (y + i >= _height) return false;
                    
                    if (_grid.Get(x, y + i) != null)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
        
        public Guid Owner { get; private set; }
        public byte Window { get; private set; }
        public ReadOnlyCollection<Item> Items {
            get {
                return _items.AsReadOnly();
            }
        }
        public Equipment EquipmentWindow { get; private set; }

        public long Size {
            get {
                return _width * _height * _pages.Length;
            }
        }

        private readonly Page[] _pages;
        private readonly IItemManager _itemManager;
        private ushort _width;
        private ushort _height;
        private readonly List<Item> _items = new List<Item>();
        private readonly IDatabaseManager _databaseManager;

        public Inventory(IItemManager itemManager, IDatabaseManager databaseManager, Guid owner, byte window, ushort width, ushort height, ushort pages)
        {
            Owner = owner;
            Window = window;

            _itemManager = itemManager;
            _databaseManager = databaseManager;
            _width = width;
            _height = height;

            // Initialize pages
            _pages = new Page[pages];
            for (var i = 0; i < pages; i++)
            {
                _pages[i] = new Page(_itemManager, width, height);
            }
            
            // Initialize equipment
            EquipmentWindow = new Equipment(Owner, Size);
        }

        public async Task Load()
        {
            _items.Clear();
            
            var pageSize = _width * _height;
            await foreach(var item in Item.GetItems(_databaseManager, Owner, Window))
            {
                // Calculate page
                var page = item.Position / pageSize;
                if (page >= _pages.Length)
                {
                    if (!EquipmentWindow.SetItem(item))
                    {
                        Log.Warning(
                            $"Failed to load item {item.Id} in position {item.Position} as it is outside the inventory!");
                    }

                    continue;
                }

                if (!_pages[page].Place(item, item.Position - page * pageSize))
                {
                    Log.Warning($"Failed to place item in inventory");
                }
                else
                {
                    _items.Add(item);
                }
            }
        }

        public async Task<bool> PlaceItem(Item instance)
        {
            for(var i = 0; i < _pages.Length; i++)
            {
                var page = _pages[i];
                
                var pos = page.Place(instance);
                if (pos != -1)
                {
                    await instance.Set(Owner, Window, (uint) (pos + i * _width * _height));
                    return true;
                }
            }

            // No space left in inventory
            return false;
        }

        public async Task<bool> PlaceItem(Item item, ushort position)
        {
            var pageSize = _width * _height;
            var page = position / pageSize;
            if (page >= _pages.Length)
            {
                return false;
            }

            if (_pages[page].Place(item, position - page * pageSize))
            {
                _items.Add(item);
                await item.Set(Owner, Window, position);
                return true;
            }

            return false;
        }

        public void RemoveItem(Item item)
        {
            var pageSize = _width * _height;
            var page = item.Position / pageSize;
            if (page >= _pages.Length)
            {
                return;
            }

            _items.Remove(item);
            _pages[page].RemoveItem(item.Position - page * pageSize);
        }

        public Item GetItem(ushort position)
        {
            var pageSize = _width * _height;
            var page = position / pageSize;
            if (page >= _pages.Length)
            {
                return null;
            }

            return _pages[page].GetItem(position - page * pageSize);
        }

        public bool IsSpaceAvailable(Item item, ushort position)
        {
            var pageSize = _width * _height;
            var page = position / pageSize;
            if (page >= _pages.Length)
            {
                return false;
            }

            return _pages[page].IsSpaceAvailable(item, position - page * pageSize);
        }

        public void MoveItem(Item item, ushort fromPosition, ushort position)
        {
            var pageSize = _width * _height;

            var fromPage = fromPosition / pageSize;
            if (fromPage >= _pages.Length)
            {
                Log.Debug("Invalid from position");
                return;
            }

            var toPage = position / pageSize;
            if (toPage >= _pages.Length)
            {
                Log.Debug("Invalid to position");
                return;
            }

            if (!_pages[fromPage].RemoveItem(fromPosition - fromPage * pageSize))
            {
                Log.Debug("Failed to remove item");
            }

            if (!_pages[toPage].Place(item, position - toPage * pageSize))
            {
                Log.Debug("Failed to place item");
            }
        }
    }
}