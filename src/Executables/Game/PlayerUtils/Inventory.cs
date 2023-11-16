using System.Collections.ObjectModel;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.Caching;
using QuantumCore.Core.Utils;
using QuantumCore.Extensions;
using QuantumCore.Game.Persistence;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace QuantumCore.Game.PlayerUtils
{
    public static class InventoryConstants
    {
        public const ushort DEFAULT_INVENTORY_WIDTH = 5;
        public const ushort DEFAULT_INVENTORY_HEIGHT = 9;
        public const ushort DEFAULT_INVENTORY_PAGES = 2;
    }

    public class Inventory : IInventory
    {
        private class Page
        {
            private readonly IItemManager _itemManager;
            private ushort _width;
            private ushort _height;

            private readonly Grid<ItemInstance?> _grid;

            public Page(IItemManager itemManager, ushort width, ushort height)
            {
                _itemManager = itemManager;
                _width = width;
                _height = height;

                _grid = new Grid<ItemInstance?>(_width, _height);
            }

            public ItemInstance? GetItem(long position)
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
            public long Place(ItemInstance item)
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

            public bool Place(ItemInstance item, uint x, uint y)
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

            public bool Place(ItemInstance item, long position)
            {
                if (position < 0) return false;
                if (position >= _width * _height) return false;

                var x = (uint)(position % _width);
                var y = (uint)(position / _width);

                return Place(item, x, y);
            }

            public bool IsSpaceAvailable(ItemInstance item, long position)
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

        public event EventHandler<SlotChangedEventArgs>? OnSlotChanged;
        public Guid Owner { get; private set; }
        public byte Window { get; private set; }

        public ReadOnlyCollection<ItemInstance> Items {
            get {
                return _items.AsReadOnly();
            }
        }

        public IEquipment EquipmentWindow { get; private set; }


        public long Size {
            get {
                return _width * _height * _pages.Length;
            }
        }

        private readonly Page[] _pages;
        private readonly IItemManager _itemManager;
        private ushort _width;
        private ushort _height;
        private readonly List<ItemInstance> _items = new List<ItemInstance>();
        private readonly ICacheManager _cacheManager;
        private readonly ILogger _logger;
        private readonly IItemRepository _itemRepository;

        public Inventory(IItemManager itemManager, ICacheManager cacheManager, ILogger logger,
            IItemRepository itemRepository, Guid owner, byte window, ushort width, ushort height, ushort pages)
        {
            Owner = owner;
            Window = window;

            _itemManager = itemManager;
            _cacheManager = cacheManager;
            _logger = logger;
            _itemRepository = itemRepository;
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
            await foreach(var item in _itemRepository.GetItems(_cacheManager, Owner, Window))
            {
                // Calculate page
                var page = item.Position / pageSize;
                if (page >= _pages.Length)
                {
                    if (!EquipmentWindow.SetItem(item))
                    {
                        Log.Warning("Failed to load item {Id} in position {Position} as it is outside the inventory!", item.Id, item.Position);
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

        /// <summary>
        /// Places an item in the inventory in the first possible slot. Equipment items will be placed in the inventory
        /// too and not be equipped by default. If no space is available items may be equipped.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>True if placement was successful. False if no space is available</returns>
        public async Task<bool> PlaceItem(ItemInstance item)
        {
            for(var i = 0; i < _pages.Length; i++)
            {
                var page = _pages[i];

                var position = page.Place(item);
                if (position != -1)
                {
                    await item.Set(_cacheManager, Owner, Window, (uint) (position + i * _width * _height));
                    _items.Add(item);


                    var wearSlot = _itemManager.GetWearSlot(item.ItemId);
                    if (wearSlot is not null && position == EquipmentWindow.GetWearPosition(_itemManager, item.ItemId))
                    {
                        // if item is now "equipped"
                        OnSlotChanged?.Invoke(this, new SlotChangedEventArgs(item, wearSlot.Value));
                    }
                    return true;
                }
            }

            // No space left in inventory
            return false;
        }

        /// <summary>
        /// Places an item in the inventory at the given position. The position must be inside a valid inventory window.
        /// Equipment window is not valid.
        /// </summary>
        /// <param name="item">Instance to place in inventory</param>
        /// <param name="position">Where to place it</param>
        /// <returns>True if placement was successful. May be false if the slot is occupied</returns>
        public async Task<bool> PlaceItem(ItemInstance item, ushort position)
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
                await item.Set(_cacheManager, Owner, Window, position);
                return true;
            }

            return false;
        }

        public void RemoveItem(ItemInstance item)
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

        public ItemInstance? GetItem(ushort position)
        {
            var pageSize = _width * _height;
            var page = position / pageSize;
            if (page >= _pages.Length)
            {
                return null;
            }

            return _pages[page].GetItem(position - page * pageSize);
        }

        public bool IsSpaceAvailable(ItemInstance item, ushort position)
        {
            var pageSize = _width * _height;
            var page = position / pageSize;
            if (page >= _pages.Length)
            {
                return false;
            }

            return _pages[page].IsSpaceAvailable(item, position - page * pageSize);
        }

        public void MoveItem(ItemInstance item, ushort fromPosition, ushort position)
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

        public void SetEquipment(ItemInstance item, ushort position)
        {
            EquipmentWindow.SetItem(item, position);
            OnSlotChanged?.Invoke(this, new SlotChangedEventArgs(item, _itemManager.GetWearSlot(item.ItemId)!.Value));
        }
    }
}
