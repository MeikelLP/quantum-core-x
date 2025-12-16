using CommandLine;
using Microsoft.Extensions.Options;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types.Skills;
using QuantumCore.Core.Utils;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Persistence;

namespace QuantumCore.Game.Commands;

[Command("item", "Puts the given item in the inventory")]
public class ItemCommand : ICommandHandler<ItemCommandOptions>
{
    private readonly IItemManager _itemManager;
    private readonly IItemRepository _itemRepository;
    private readonly ISkillManager _skillManager;
    private readonly SkillsOptions _skillsOptions;

    public ItemCommand(IItemManager itemManager, IItemRepository itemRepository, ISkillManager skillManager,
        IOptions<GameOptions> gameOptions)
    {
        _itemManager = itemManager;
        _itemRepository = itemRepository;
        _skillManager = skillManager;
        _skillsOptions = gameOptions.Value.Skills;
    }

    public async Task ExecuteAsync(CommandContext<ItemCommandOptions> context)
    {
        var item = _itemManager.GetItem(context.Arguments.ItemId);
        if (item is null)
        {
            context.Player.SendChatInfo("Item not found");
            return;
        }

        // todo: Move to "instantiation of item" logic ?
        if (item.Id == _skillsOptions.GenericSkillBookId)
        {
            var skillBookId = 0U;
            do
            {
                skillBookId = CoreRandom.GenerateUInt32(1, 112);

                if (!Enum.TryParse<ESkill>(skillBookId.ToString(), out var skillId))
                {
                    continue;
                }

                var skill = _skillManager.GetSkill(skillId);
                if (skill is null)
                {
                    continue;
                }

                break;
            } while (true);

            var bookId = _skillsOptions.SkillBookStartId + skillBookId;

            item = _itemManager.GetItem(bookId);
            if (item is null)
            {
                context.Player.SendChatInfo($"Skillbook ({bookId}) not found");
                return;
            }
        }

        var instance = new ItemInstance
        {
            ItemId = item.Id, Count = context.Arguments.Count, PlayerId = context.Player.Player.Id,
        };
        if (!await context.Player.Inventory.PlaceItem(instance))
        {
            context.Player.SendChatInfo("No place in inventory");
            return;
        }

        await instance.Persist(_itemRepository);
        context.Player.SendItem(instance);
    }
}

public class ItemCommandOptions
{
    [Value(0)] public uint ItemId { get; set; }

    [Value(1)] public byte Count { get; set; } = 1;
}
