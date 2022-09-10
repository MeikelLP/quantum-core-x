using System;

namespace QuantumCore.Game.Quest.QuestCondition;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class OnceAttribute : Condition
{
    public override bool Evaluate(Quest quest)
    {
        // todo
        return true;
    }
}