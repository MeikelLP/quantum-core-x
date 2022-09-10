using System;

namespace QuantumCore.Game.Quest.QuestCondition;

public abstract class Condition : Attribute
{
    public abstract bool Evaluate(Quest quest);
}