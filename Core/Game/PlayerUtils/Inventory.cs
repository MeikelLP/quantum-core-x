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
    public class Inventory
    {
        private class Page
        {
            private ushort _width;
            private ushort _height;

            private readonly Grid<Item> _grid;

            public Page(ushort width, ushort height)
            {
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

                var itemSize = 1; // todo: Look up item proto size
                for (byte i = 0; i < itemSize; i++)
                {
                    _grid.Set(x, y + i, null);
                }

                return true;
            }

            public bool Place(Item item, long position)
            {
                if (position < 0) return false;
                if (position >= _width * _height) return false;

                var x = (uint)(position % _width);
                var y = (uint)(position / _width);

                var itemSize = 1; // todo: Look up item proto size
                
                // Check if all required positions are free and in bounds
                for (byte i = 0; i < itemSize; i++) 
                {
                    if (_grid.Get(x, y + i) != null)
                    {
                        return false;
                    }
                }

                // Place the item
                for (byte i = 0; i < itemSize; i++)
                {
                    _grid.Set(x, y, item);
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

        private readonly Page[] _pages;
        private ushort _width;
        private ushort _height;
        private readonly List<Item> _items = new List<Item>();
        
        public Inventory(Guid owner, byte window, ushort width, ushort height, ushort pages)
        {
            Owner = owner;
            Window = window;

            _width = width;
            _height = height;
            
            // Initialize pages
            _pages = new Page[pages];
            for (var i = 0; i < pages; i++)
            {
                _pages[i] = new Page(width, height);
            }
        }

        public async Task Load()
        {
            _items.Clear();
            
            var pageSize = _width * _height;
            await foreach(var item in Item.GetItems(Owner, Window))
            {
                // Calculate page
                var page = item.Position / pageSize;
                if (page >= _pages.Length)
                {
                    Log.Warning($"Failed to load item {item.Id} in position {item.Position} as it is outside the inventory!");
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
    }
}