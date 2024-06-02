using CommandLine;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game;
using QuantumCore.Caching;
using QuantumCore.Core.Utils;
using QuantumCore.Extensions;
using QuantumCore.Game.Extensions;

namespace QuantumCore.Game.Commands
{
    [Command("item", "Puts the given item in the inventory")]
    public class ItemCommand : ICommandHandler<ItemCommandOptions>
    {
        private readonly IItemManager _itemManager;
        private readonly ICacheManager _cacheManager;
        private readonly ISkillManager _skillManager;

        public ItemCommand(IItemManager itemManager, ICacheManager cacheManager, ISkillManager skillManager)
        {
            _itemManager = itemManager;
            _cacheManager = cacheManager;
            _skillManager = skillManager;
        }

        public async Task ExecuteAsync(CommandContext<ItemCommandOptions> context)
        {
            var item = _itemManager.GetItem(context.Arguments.ItemId);
            if (item == null)
            {
                context.Player.SendChatInfo("Item not found");
                return;
            }

            // todo: Move to "instantiation of item" logic ?
            if (item.Id == SkillsConstants.GENERIC_SKILLBOOK_ID)
            {
                var skillBookId = 0U;
                do
                {
                    skillBookId = CoreRandom.GenerateUInt32(1, 112);

                    var skill = _skillManager.GetSkill(skillBookId);
                    if (skill == null)
                    {
                        continue;
                    }

                    break;

                } while (true);
                
                var bookId = SkillsConstants.SKILLBOOK_START_ID + skillBookId;
                
                item = _itemManager.GetItem(bookId);
                if (item == null)
                {
                    context.Player.SendChatInfo($"Skillbook ({bookId}) not found");
                    return;
                }
            }

            var instance = new ItemInstance {Id = Guid.NewGuid(), ItemId = item.Id, Count = context.Arguments.Count};
            if (!await context.Player.Inventory.PlaceItem(instance))
            {
                context.Player.SendChatInfo("No place in inventory");
                return;
            }

            await instance.Persist(_cacheManager);
            context.Player.SendItem(instance);
        }
    }

    public class ItemCommandOptions
    {
        [Value(0)] public uint ItemId { get; set; }

        [Value(1)] public byte Count { get; set; } = 1;
    }
}
