using QuantumCore.API.Core.Types;
using Xunit;

namespace Core.Tests;

public class ValueObjectTests
{
    private class TestValueObject(int value) : ValueObject<int>(value)
    { }


    [Fact]
    private void ValueObject_EncapsulatesValue()
    {
        var valueObject = new TestValueObject(5);
        Assert.Equal(5, valueObject.Value);
    }

    [Fact]
    private void Equals_ReturnsTrue_ForEqualValueObjects()
    {
        var valueObject1 = new TestValueObject(5);
        var valueObject2 = new TestValueObject(5);
        Assert.True(valueObject1.Equals(valueObject2));
    }

    [Fact]
    private void Equals_ReturnsFalse_ForDifferentValueObjects()
    {
        var valueObject1 = new TestValueObject(5);
        var valueObject2 = new TestValueObject(10);
        Assert.False(valueObject1.Equals(valueObject2));
    }

    [Fact]
    private void GetHashCode_ReturnsSameHashCode_ForEqualValueObjects()
    {
        var valueObject1 = new TestValueObject(5);
        var valueObject2 = new TestValueObject(5);
        Assert.Equal(valueObject1.GetHashCode(), valueObject2.GetHashCode());
    }

    [Fact]
    private void ToString_ReturnsValueToString()
    {
        var valueObject = new TestValueObject(5);
        Assert.Equal("5", valueObject.ToString());
    }

    [Fact]
    private void OperatorEquals_ReturnsTrue_ForEqualValueObjects()
    {
        var valueObject1 = new TestValueObject(5);
        var valueObject2 = new TestValueObject(5);
        Assert.True(valueObject1 == valueObject2);
    }

    [Fact]
    private void OperatorNotEquals_ReturnsTrue_ForDifferentValueObjects()
    {
        var valueObject1 = new TestValueObject(5);
        var valueObject2 = new TestValueObject(10);
        Assert.True(valueObject1 != valueObject2);
    }

    [Fact]
    private void ImplicitConversion_ReturnsEncapsulatedValue()
    {
        var valueObject = new TestValueObject(5);
        int value = valueObject;
        Assert.Equal(5, value);
    }
}