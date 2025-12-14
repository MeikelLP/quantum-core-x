using QuantumCore.API.Core.Models;
using QuantumCore.API.Extensions;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types.Players;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Persistence;

namespace QuantumCore.Game.Commands;

[Command("full_set", "Gives yourself a full set of equipment for your class +9")]
public class FullSetCommand : ICommandHandler
{
    private readonly IItemRepository _itemRepository;

    private static readonly uint HairBaseId = 73001;
    
    private static readonly uint[] SharedItems = [13069, 17209, 14209, 15229, 16209];
    private static readonly uint[] WarriorItems = [12249, 189, 3169, 11299, HairBaseId];
    private static readonly uint[] NinjaItems = [12389, 189, 1139, 2189, 11499, HairBaseId + 250];
    private static readonly uint[] SuraItems = [12529, 189, 11699, HairBaseId + 500];
    private static readonly uint[] ShamanItems = [12669, 5129, 11899, HairBaseId + 750];

    public FullSetCommand(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }

    public async Task ExecuteAsync(CommandContext context)
    {
        foreach (var item in SharedItems)
        {
            var instance = new ItemInstance {ItemId = item, Count = 1, PlayerId = context.Player.Player.Id};
            if (!await context.Player.Inventory.PlaceItem(instance))
            {
                context.Player.SendChatInfo("No place in inventory");
                return;
            }

            await instance.Persist(_itemRepository);
            context.Player.SendItem(instance);
        }

        var jobItems = context.Player.Player.PlayerClass.GetClass() switch
        {
            EPlayerClass.Warrior => WarriorItems,
            EPlayerClass.Ninja => NinjaItems,
            EPlayerClass.Sura => SuraItems,
            EPlayerClass.Shaman => ShamanItems,
            _ => throw new ArgumentOutOfRangeException(nameof(context),
                $"No default items for player job {context.Player.Player.PlayerClass}")
        };
        foreach (var item in jobItems)
        {
            var instance = new ItemInstance {ItemId = item, Count = 1, PlayerId = context.Player.Player.Id};
            if (!await context.Player.Inventory.PlaceItem(instance))
            {
                context.Player.SendChatInfo("No place in inventory");
                return;
            }

            await instance.Persist(_itemRepository);
            context.Player.SendItem(instance);
        }
    }
}
