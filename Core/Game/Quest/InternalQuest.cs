using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        return await Choice(events.ToArray());
    }

    public void EndQuest()
    {
        SetSkin(QuestSkin.NoWindow);
        Done();
    }
}