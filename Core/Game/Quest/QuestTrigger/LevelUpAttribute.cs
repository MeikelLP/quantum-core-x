using System;
using JetBrains.Annotations;

namespace QuantumCore.Game.Quest.QuestTrigger;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class LevelUpAttribute : Attribute
{
    
}