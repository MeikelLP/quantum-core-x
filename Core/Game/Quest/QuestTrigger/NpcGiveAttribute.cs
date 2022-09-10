using System;
using JetBrains.Annotations;

namespace QuantumCore.Game.Quest.QuestTrigger;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class NpcGiveAttribute : Attribute
{
    public uint NpcId { get; private set; }
    public string Name { get; private set; }

    public NpcGiveAttribute(uint npcId, string name = null)
    {
        NpcId = npcId;
        Name = name;
    }
}