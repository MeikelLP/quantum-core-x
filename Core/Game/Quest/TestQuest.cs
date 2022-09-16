using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Database;
using Serilog;

namespace QuantumCore.Game.Quest;

[Quest]
public class TestQuest : Quest
{
    private const uint Vnum1 = 101;
    private const uint Vnum2 = 103;

    private QuestLetter _letter;

    public TestQuest(QuestState state, IPlayerEntity player) : base(state, player)
    {
    }

    public override void Init()
    {
        if (Player.GetPoint(EPoints.Level) >= 10 && State.Get<uint>("monster") == 0)
        {
            _letter = CreateQuestLetter("Test Quest", QuestLetter);
            _letter.CounterName = "Test";
            _letter.CounterValue = 10;
            _letter.Send();
        }
    }

    [QuestTrigger.LevelUp]
    [QuestCondition.Level(10)]
    [QuestCondition.Once]
    public void LevelUp()
    {
        if (_letter == null)
        {
            // The letter should normally never exists at this point, but just to make sure we check it here
            _letter = CreateQuestLetter("Test Quest", QuestLetter);
        }
        
        _letter.Send();
        Log.Debug("TestQuest: Level Up trigger with level 10");
    }

    private void QuestLetter()
    {
        Text("Please visit npc " + MonsterManager.GetMonster(20354).TranslatedName);
        Done();
    }

    [QuestTrigger.NpcClick(20354)]
    [QuestCondition.Level(10)]
    [QuestCondition.State("monster", 0)]
    public async void NpcTalk()
    {
        Text("Hello World from QuantumCore!");
        Text("This is using the current work in progress");
        Text("Quest API.");
        await Next();
        
        Text("Please select a monster you want to have to kill");
        var choice = await Choice(
            "10 x " + MonsterManager.GetMonster(Vnum1).TranslatedName,
            "10 x " + MonsterManager.GetMonster(Vnum2).TranslatedName);

        State.Set("monster", choice == 1 ? Vnum1 : Vnum2);
        State.Set("count", 10);
        
        SetSkin(QuestSkin.NoWindow);
        Done();
    }

    [QuestTrigger.MonsterKill(Vnum1)]
    [QuestTrigger.MonsterKill(Vnum2)]
    [QuestCondition.State("count", 0, QuestCondition.StateAttribute.Comparator.Greater)]
    public void MonsterKill(QuestTrigger.MonsterKillAttribute trigger)
    {
        Log.Debug("TestQuest: Monster kill trigger");
        
        var selectedMonster = State.Get<uint>("monster");
        if (trigger.MonsterId != selectedMonster)
        {
            return;
        }
        
        var count = State.Get<int>("count");
        count--;
        State.Set("count", count);
        
        if (count <= 0)
        {
            CreateQuestLetter("Test Quest", FinishQuest);
        }
    }

    private void FinishQuest()
    {
        Text("Thanks for helping me!");
        Player.AddPoint(EPoints.Experience, 10_000);
        
        Done();
        ExitQuest();
    }

    [QuestTrigger.NpcGive(20016)]
    public async void TestGive(Item item)
    {
        var proto = ItemManager.Instance.GetItem(item.ItemId);
        
        Text($"Thanks for giving me the item {proto.TranslatedName}.");
        Done();
    }
}