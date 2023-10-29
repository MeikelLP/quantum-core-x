using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using QuantumCore.API.Game.World;
using QuantumCore.Game;
using QuantumCore.Game.Services;
using QuantumCore.Game.World;
using Xunit;

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
                new SpawnMember { Id = 101 },
                new SpawnMember { Id = 101 }
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
                new SpawnMember { Id = 2492 },
                new SpawnMember { Id = 2414 },
                new SpawnMember { Id = 2414 },
                new SpawnMember { Id = 2411 },
                new SpawnMember { Id = 2411 },
                new SpawnMember { Id = 2411 }
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
                new SpawnGroupCollectionMember { Id = 101, Amount = 1 },
                new SpawnGroupCollectionMember { Id = 171, Amount = 1 }
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
                new SpawnGroupCollectionMember { Id = 101, Amount = 1 },
                new SpawnGroupCollectionMember { Id = 171, Amount = 1 }
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
                new SpawnGroupCollectionMember { Id = 101, Amount = 1 },
                new SpawnGroupCollectionMember { Id = 171, Amount = 1 }
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
                new SpawnGroupCollectionMember { Id = 112, Amount = 1 },
                new SpawnGroupCollectionMember { Id = 113, Amount = 1 }
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

    [Fact]
    public async Task Drop_Mob_DropSingle()
    {
        var input = new StringReader("""
                             Group	Abc
                             {
                             	Mob	101
                             	Type	drop
                             	1	10	1	0.09
                             }
                             """);

        var result = await ParserUtils.GetDropsForBlockAsync(input);
        result.Should().NotBeNull();
        var drops = result!.Value;
        drops.Key.Should().Be(101);
        drops.Value.Should().HaveCount(1);
        drops.Value[0].ItemProtoId.Should().Be(10);
        drops.Value[0].Chance.Should().BeApproximately(0.0009f, 0.0005f);
    }

    [Fact]
    public async Task Drop_Mob_DropMultiple()
    {
        var input = new StringReader("""
                             Group	Abc
                             {
                             	Mob	101
                             	Type	drop
                             	1	10	1	0.1
                             	2	11	2	1
                             }
                             """);

        var result = await ParserUtils.GetDropsForBlockAsync(input);
        result.Should().NotBeNull();
        var drops = result!.Value;
        drops.Key.Should().Be(101);
        drops.Value.Should().HaveCount(2);
        drops.Value[0].Should().BeEquivalentTo(new DropEntry(10, 0.001f));
        drops.Value[1].Should().BeEquivalentTo(new DropEntry(11, 0.01f, Amount: 2));
    }

    [Fact]
    public async Task Drop_MultipleMob()
    {
        var input = new StringReader("""
                             Group	Abc
                             {
                             	Mob	101
                             	Type	drop
                             	1	10	1	0.1
                             }
                             Group	Abc2
                             {
                             	Mob	102
                             	Type	drop
                             	1	10	1	0.1
                             }
                             """);

        var result = await ParserUtils.GetDropsForBlockAsync(input);
        result.Should().NotBeNull();
        var drops = result!.Value;
        drops.Key.Should().Be(101);
        drops.Value.Should().HaveCount(1);
        drops.Value.Should().Contain(new DropEntry(10, 0.001f));
    }

    [Fact]
    public async Task Drop_FloatForUint()
    {
        var input = new StringReader("""
                             Group	Abc
                             {
                             	Kill_drop	4.0
                             	Mob	101
                             	Type	drop
                             	1	10	1	100
                             }
                             """);

        var result = await ParserUtils.GetDropsForBlockAsync(input);
        result.Should().NotBeNull();
        var drops = result!.Value;
        drops.Key.Should().Be(101);
        drops.Value.Should().HaveCount(1);
        drops.Value.Should().Contain(new DropEntry(10, 1, MinKillCount: 4));
    }

    [Fact]
    public async Task Drop_StringItemId_WillBeIgnored()
    {
        var input = new StringReader("""
                             Group	Abc
                             {
                             	Mob	101
                             	Type	drop
                             	1	10	1	100
                             	2	Blub	1	100
                             }
                             """);

        var result = await ParserUtils.GetDropsForBlockAsync(input);
        result.Should().NotBeNull();
        var drops = result!.Value;
        drops.Key.Should().Be(101);
        drops.Value.Should().HaveCount(1);
        drops.Value.Should().Contain(new DropEntry(10, 1));
    }
}
