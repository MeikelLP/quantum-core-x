using FluentAssertions;
using QuantumCore.API.Game.World;
using QuantumCore.Game.World;
using Xunit;
using ParserUtils = QuantumCore.Game.ParserUtils;

namespace Core.Tests;

public class ParserTests
{
    [Theory]
    [InlineData("//r	751	311	10	10	0	0	5s	100	1	101")]
    [InlineData("	//r	751	311	10	10	0	0	5s	100	1	101")]
    [InlineData("	")]
    [InlineData("")]
    [InlineData("//")]
    public void Spawn_Null(string input)
    {
        var result = ParserUtils.GetSpawnFromLine(input);

        result.Should().BeNull();
    }

    [Fact]
    public void Spawn_Regular()
    {
        const string input = "r	751	311	10	10	0	0	5s	100	1	101";
        var result = ParserUtils.GetSpawnFromLine(input);

        result.Should().BeEquivalentTo(new SpawnPoint
        {
            Type = ESpawnPointType.GroupCollection,
            IsAggressive = false,
            X = 751,
            Y = 311,
            RangeX = 10,
            RangeY = 10,
            Direction = 0,
            MaxAmount = 1,
            Chance = 100,
            Monster = 101,
            RespawnTime = 5
        });
    }

    [Fact]
    public void Spawn_GroupAggressive()
    {
        const string input = "ga	188	470	10	10	0	0	0s	100	1	1005";
        var result = ParserUtils.GetSpawnFromLine(input);

        result.Should().BeEquivalentTo(new SpawnPoint
        {
            Type = ESpawnPointType.Group,
            IsAggressive = true,
            X = 188,
            Y = 470,
            RangeX = 10,
            RangeY = 10,
            Direction = 0,
            MaxAmount = 1,
            Chance = 100,
            Monster = 1005,
            RespawnTime = 0
        });
    }

    [Fact]
    public async Task Group_Regular()
    {
        var input = new StringReader("""
                                     Group	Test
                                     {
                                         Vnum	101
                                         Leader	Test	101
                                         1	Test	101
                                         2	Test	101
                                     }
                                     """);
        var result = await ParserUtils.GetSpawnGroupFromBlock(input);

        result.Should().BeEquivalentTo(new SpawnGroup
        {
            Name = "Test",
            Id = 101,
            Leader = 101,
            Members =
            {
                new SpawnMember {Id = 101},
                new SpawnMember {Id = 101}
            }
        });
    }

    [Fact]
    public async Task Group_Whitespace()
    {
        var input = new StringReader("""
                                     Group   GroupName
                                     {
                                     	Vnum    2430
                                     		Leader  Leader    2493
                                     		1   Mob1  2492
                                     		2   Mob2  2414
                                     		3   Mob2  2414
                                     		4   Mob3  2411
                                     		5   Mob3  2411
                                     		6   Mob3  2411
                                     }
                                     """);
        var result = await ParserUtils.GetSpawnGroupFromBlock(input);

        result.Should().BeEquivalentTo(new SpawnGroup
        {
            Name = "GroupName",
            Id = 2430,
            Leader = 2493,
            Members =
            {
                new SpawnMember {Id = 2492},
                new SpawnMember {Id = 2414},
                new SpawnMember {Id = 2414},
                new SpawnMember {Id = 2411},
                new SpawnMember {Id = 2411},
                new SpawnMember {Id = 2411}
            }
        });
    }

    [Fact]
    public async Task Group_WhitespaceInsteadOfTab()
    {
        var input = new StringReader("""
                                     Group	GroupName
                                     {
                                     	Vnum	2430
                                     	Leader	Leader	2493
                                     	1	Mob1	2447
                                     	2	Mob2 2447
                                     	3	Mob 3	2513
                                     }
                                     """);
        var result = await ParserUtils.GetSpawnGroupFromBlock(input);

        result.Should().BeEquivalentTo(new SpawnGroup
        {
            Name = "GroupName",
            Id = 2430,
            Leader = 2493,
            Members =
            {
                new SpawnMember {Id = 2447},
                new SpawnMember {Id = 2447},
                new SpawnMember {Id = 2513}
            }
        });
    }

    [Fact]
    public async Task GroupCollection_Regular()
    {
        var input = new StringReader("""
                                     Group	a1_01
                                     {
                                     	Vnum	101
                                     	1	101	1
                                     	2	171	1
                                     }
                                     """);
        var result = await ParserUtils.GetSpawnGroupCollectionFromBlock(input);

        result.Should().BeEquivalentTo(new SpawnGroupCollection
        {
            Name = "a1_01",
            Id = 101,
            Groups =
            {
                new SpawnGroupCollectionMember {Id = 101, Amount = 1},
                new SpawnGroupCollectionMember {Id = 171, Amount = 1}
            }
        });
    }

    [Fact]
    public async Task GroupCollection_Multiple()
    {
        var input = new StringReader("""
                                     Group	a1_01
                                     {
                                     	Vnum	101
                                     	1	101	1
                                     	2	171	1
                                     }
                                     			
                                     Group	a1_02
                                     {
                                     	Vnum	102
                                     	1	102	1
                                     	2	171	1
                                     }
                                     """);
        var result = await ParserUtils.GetSpawnGroupCollectionFromBlock(input);

        result.Should().BeEquivalentTo(new SpawnGroupCollection
        {
            Name = "a1_01",
            Id = 101,
            Groups =
            {
                new SpawnGroupCollectionMember {Id = 101, Amount = 1},
                new SpawnGroupCollectionMember {Id = 171, Amount = 1}
            }
        });
    }

    [Fact]
    public async Task GroupCollection_EmptyLines()
    {
        var input = new StringReader("""
                                     Group	a1_01
                                     {
                                     	Vnum	101
                                     	1	101	1
                                     	2	171	1
                                     }
                                     			
                                     			
                                     """);
        var result = await ParserUtils.GetSpawnGroupCollectionFromBlock(input);

        result.Should().BeEquivalentTo(new SpawnGroupCollection
        {
            Name = "a1_01",
            Id = 101,
            Groups =
            {
                new SpawnGroupCollectionMember {Id = 101, Amount = 1},
                new SpawnGroupCollectionMember {Id = 171, Amount = 1}
            }
        });
    }

    [Fact]
    public async Task GroupCollection_EmptyLinesInside()
    {
        var input = new StringReader("""
                                     Group	a1_05
                                     {
                                     	Vnum	105
                                     			
                                     	1	112	1
                                     	2	113	1
                                     }
                                     """);
        var result = await ParserUtils.GetSpawnGroupCollectionFromBlock(input);

        result.Should().BeEquivalentTo(new SpawnGroupCollection
        {
            Name = "a1_05",
            Id = 105,
            Groups =
            {
                new SpawnGroupCollectionMember {Id = 112, Amount = 1},
                new SpawnGroupCollectionMember {Id = 113, Amount = 1}
            }
        });
    }

    [Fact]
    public async Task GroupCollection_WithoutAmount()
    {
        var input = new StringReader("""
                                     Group	a1_05
                                     {
                                     	Vnum	105
                                     	1	112
                                     }
                                     """);
        var result = await ParserUtils.GetSpawnGroupCollectionFromBlock(input);

        result.Should().BeEquivalentTo(new SpawnGroupCollection
        {
            Name = "a1_05",
            Id = 105,
            Groups =
            {
                new SpawnGroupCollectionMember {Id = 112, Amount = 1}
            }
        });
    }

    [Fact]
    public async Task GroupCollection_NoContent()
    {
        var input = new StringReader("""
                                     			
                                     """);
        var result = await ParserUtils.GetSpawnGroupCollectionFromBlock(input);

        result.Should().BeNull();
    }
}
