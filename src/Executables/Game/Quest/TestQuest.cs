using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Quest;

[Quest]
public class TestQuest : Quest
{
    private readonly IItemManager _itemManager;

    public TestQuest(QuestState state, IPlayerEntity player, IItemManager itemManager) : base(state, player)
    {
        _itemManager = itemManager;
    }

    public override void Init()
    {
        // todo invent api for register npc click event on player
        GameEventManager.RegisterNpcClickEvent("Test Quest", 20354, Test, player => player.Vid == Player.Vid);
        GameEventManager.RegisterNpcGiveEvent("Test Quest", 20016, (player, item) =>
        {
            TestGive(player, item);
            return Task.CompletedTask;
        }, (player, _) => player.Vid == Player.Vid);
    }

    private async Task Test(IPlayerEntity player)
    {
        Text("Hello World from QuantumCore!");
        Text("This is using the current work in progress");
        Text("Quest API.");
        Next();

        Text("This is the second page showing how to easily");
        Text("using await to wait for user response");
        var choice = await Choice(false, "1st option", "2nd option");

        Text($"You've chosen: {choice}");
        Done();
    }

    private void TestGive(IPlayerEntity player, ItemInstance item)
    {
        var proto = _itemManager.GetItem(item.ItemId);

        if (proto == null)
        {
            Text("Failure: Could not find item.");
        }
        else
        {
            Text($"Thanks for giving me the item {proto.TranslatedName}.");
            player.Inventory.PlaceItem(item);
        }
        Done();
    }
}
