using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types.Quest;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Quest;

[Quest]
public class InternalQuest : Quest
{
    public InternalQuest(QuestState state, IPlayerEntity player) : base(state, player)
    {
    }

    public override void Init()
    {
    }

    public async Task<byte> SelectQuest(IEnumerable<string> events)
    {
        return await Choice(false, events.ToArray());
    }

    public void EndQuest()
    {
        SetSkin(QuestSkin.NoWindow);
        Done();
    }
}