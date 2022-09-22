﻿using System;
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
        return await Choice(false, events.ToArray());
    }

    public async Task EndQuest()
    {
        SetSkin(QuestSkin.NoWindow);
        await Done();
    }
}