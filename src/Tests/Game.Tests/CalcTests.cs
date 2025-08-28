using AwesomeAssertions;
using QuantumCore.Game.World.Entities;

namespace Game.Tests;

public class CalcTests
{
    [Theory]
    [InlineData(0, 10, 40, 1)]
    [InlineData(0, 15, 40, 1)]
    [InlineData(0, 19, 40, 1)]
    [InlineData(0, 20, 40, 2)]
    [InlineData(0, 5, 40, 0)]
    [InlineData(0, 40, 40, 0)] // level up is not partial level up
    public void CheckIsPartialLevelUp(uint before, uint after, uint required, int output)
    {
        PlayerEntity.CalcPartialLevelUps(before, after, required).Should().Be(output);
    }
}
