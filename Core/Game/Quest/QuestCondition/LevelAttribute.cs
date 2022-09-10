using System;
using QuantumCore.API.Game.Types;

namespace QuantumCore.Game.Quest.QuestCondition;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class LevelAttribute : Condition
{
    public byte Level { get; private set; }

    public LevelAttribute(byte level)
    {
        Level = level;
    }

    public override bool Evaluate(Quest quest)
    {
        return quest.Player.GetPoint(EPoints.Level) >= Level;
    }
}