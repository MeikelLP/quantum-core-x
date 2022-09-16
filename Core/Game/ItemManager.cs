using System;
using System.Linq;
using QuantumCore.Core.Types;
using QuantumCore.Database;

namespace QuantumCore.Game
{
    /// <summary>
    /// Manage all static data related to items
    /// </summary>
    public class ItemManager : IItemManager
    {
        private static ItemProto _proto;
        public static IItemManager Instance { get; internal set; } = new ItemManager();

        /// <summary>
        /// Query for a specific item definition by it's id
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>The item definition or null if the item is not known</returns>
        public ItemProto.Item GetItem(uint id)
        {
            return _proto.Content.Data.Items.FirstOrDefault(item => item.Id == id);
        }
        
        /// <summary>
        /// Try to load the item_proto file
        /// </summary>
        public void Load()
        {
            _proto = ItemProto.FromFile("data/item_proto");
        }

        /// <summary>
        /// Create an instance for the given item definition.
        /// The owner, window, position will left empty.
        /// Also the object won't get stored without calling Item.Persist()!
        /// </summary>
        /// <param name="proto">Item definition to create</param>
        /// <param name="count">Number of items on this stack</param>
        /// <returns>Item instance</returns>
        public Item CreateItem(ItemProto.Item proto, byte count = 1)
        {
            return new Item {
                Id = Guid.NewGuid(),
                ItemId = proto.Id,
                Count = count
            };
        }
    }
}