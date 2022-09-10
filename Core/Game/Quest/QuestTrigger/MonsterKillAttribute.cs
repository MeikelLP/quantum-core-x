using System;
using JetBrains.Annotations;

namespace QuantumCore.Game.Quest.QuestTrigger;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class MonsterKillAttribute : Attribute
{
    public uint MonsterId { get; private set; }

    public MonsterKillAttribute(uint monsterId)
    {
        MonsterId = monsterId;
    }
}