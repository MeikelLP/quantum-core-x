using AwesomeAssertions;
using QuantumCore.Game.Drops;

namespace Game.Tests;

public class DropTests
{
    [Fact]
    public void DropIsPickedFromWithinTheGroup()
    {
        var group = new MonsterItemGroup();
        group.AddDrop(itemProtoId: 1, count: 1, dropChance: 1, rareDropChance: 0);

        // the pick is random, so try often enough to cover every branch
        for (var i = 0; i < 100; i++)
        {
            group.GetDrop().Should().NotBeNull();
        }
    }

    [Fact]
    public void EmptyGroupDropsNothing()
    {
        new MonsterItemGroup().GetDrop().Should().BeNull();
    }
}
