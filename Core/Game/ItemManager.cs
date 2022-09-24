using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.Core.Types;

namespace QuantumCore.Game
{
    /// <summary>
    /// Manage all static data related to items
    /// </summary>
    public class ItemManager : IItemManager
    {
        private readonly ILogger<ItemManager> _logger;
        private ItemProto _proto;

        public ItemManager(ILogger<ItemManager> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Query for a specific item definition by it's id
        /// </summary>
        /// <param name="id">Item ID</param>
        /// <returns>The item definition or null if the item is not known</returns>
        [CanBeNull]
        public ItemData GetItem(uint id)
        {
            var proto = _proto.Content.Data.Items.FirstOrDefault(item => item.Id == id);

            if (proto is not null)
            {
                return new ItemData {
                    Applies = proto.Applies.Select(x => new ItemApplyData { Type = x.Type, Value = x.Value }).ToList(),
                    Flags = proto.Flags,
                    Id = proto.Id,
                    Limits = proto.Limits.Select(x => new ItemLimitData { Type = x.Type, Value = x.Value }).ToList(),
                    Name = proto.Name,
                    Size = proto.Size,
                    Sockets = proto.Sockets,
                    Specular = proto.Specular,
                    Subtype = proto.Subtype,
                    Type = proto.Type,
                    Unknown = proto.Unknown,
                    Unknown2 = proto.Unknown2,
                    Values = proto.Values,
                    AntiFlags = proto.AntiFlags,
                    BuyPrice = proto.BuyPrice,
                    ImmuneFlags = proto.ImmuneFlags,
                    SellPrice = proto.SellPrice,
                    SocketPercentage = proto.SocketPercentage,
                    TranslatedName = proto.TranslatedName,
                    UpgradeId = proto.UpgradeId,
                    UpgradeSet = proto.UpgradeSet,
                    WearFlags = proto.WearFlags,
                    MagicItemPercentage = proto.MagicItemPercentage
                };
            }

            return null;
        }

        /// <summary>
        /// Try to load the item_proto file
        /// </summary>
        public Task LoadAsync(CancellationToken token = default)
        {
            _logger.LogInformation("Loading item_proto");
            _proto = ItemProto.FromFile("data/item_proto");

            return Task.CompletedTask;
        }

        /// <summary>
        /// Create an instance for the given item definition.
        /// The owner, window, position will left empty.
        /// Also the object won't get stored without calling Item.Persist()!
        /// </summary>
        /// <param name="proto">Item definition to create</param>
        /// <param name="count">Number of items on this stack</param>
        /// <returns>Item instance</returns>
        public ItemInstance CreateItem(ItemData proto, byte count = 1)
        {
            return new ItemInstance { Id = Guid.NewGuid(), ItemId = proto.Id, Count = count };
        }
    }
}