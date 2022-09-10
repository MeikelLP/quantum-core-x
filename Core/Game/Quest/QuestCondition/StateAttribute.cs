using System;
using QuantumCore.Core.Utils;
using Serilog;
using SpanJson.Formatters.Dynamic;

namespace QuantumCore.Game.Quest.QuestCondition;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public class StateAttribute : Condition
{
    public enum Comparator
    {
        Equals,
        Greater,
        Less,
        GreaterEqual,
        LessEqual
    }
    
    public Comparator Compare { get; private set; }
    public string StateName { get; private set; }
    public object Value { get; private set; }

    public StateAttribute(string name, object value, Comparator compare = Comparator.Equals)
    {
        StateName = name;
        Value = value;
        Compare = compare;
    }

    public override bool Evaluate(Quest quest)
    {
        var state = quest.State.Get(StateName);
        if (state is null)
        {
            if (Value.GetType().IsValueType)
            {
                state = Activator.CreateInstance(Value.GetType());
            }
        } else if (state is SpanJsonDynamicUtf8Number number)
        {
            number.TryConvert(typeof(float), out state);
        }
        
        Log.Debug($"Comparing {state} ({state?.GetType()}) with {Value} ({Value?.GetType()}) (comparator is {Compare})");

        return Compare switch {
            Comparator.Equals => Equals(state, Value),
            Comparator.Greater => MathUtils.CompareTwoNumbers(state, Value) > 0,
            Comparator.Less => MathUtils.CompareTwoNumbers(state, Value) < 0,
            Comparator.GreaterEqual => MathUtils.CompareTwoNumbers(state, Value) >= 0,
            Comparator.LessEqual => MathUtils.CompareTwoNumbers(state, Value) <= 0,
            _ => throw new ArgumentOutOfRangeException(nameof(Compare))
        };
    }
}