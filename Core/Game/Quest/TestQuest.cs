using QuantumCore.API.Game.World;
using QuantumCore.Database;

namespace QuantumCore.Game.Quest;

[Quest]
public class TestQuest : Quest
{
    public TestQuest(QuestState state, IPlayerEntity player) : base(state, player)
    {
    }

    public override void Init()
    {
        // todo invent api for register npc click event on player
        GameEventManager.RegisterNpcClickEvent("Test Quest", 20354, Test, player => player.Vid == Player.Vid);
        GameEventManager.RegisterNpcGiveEvent("Test Quest", 20016, TestGive, (player, _) => player.Vid == Player.Vid);
    }

    private async void Test(IPlayerEntity player)
    {
        Text("Hello World from QuantumCore!");
        Text("This is using the current work in progress");
        Text("Quest API.");
        await Next();
        
        Text("This is the second page showing how to easily");
        Text("using await to wait for user response");
        var choice = await Choice(false, "1st option", "2nd option");
        
        Text($"You've chosen: {choice}");
        Done();
    }

    private async void TestGive(IPlayerEntity player, Item item)
    {
        var proto = ItemManager.Instance.GetItem(item.ItemId);
        
        Text($"Thanks for giving me the item {proto.TranslatedName}.");
        Done();
    }
}