using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using QuantumCore.API;
using QuantumCore.API.Game.World;
using QuantumCore.Game;
using QuantumCore.Game.Drops;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Services;
using QuantumCore.Game.World;
using Xunit;
using Xunit.Abstractions;

namespace Core.Tests;

public class ParserTests
{
    private const float CHANCE_ALLOWED_APPROXIMATION = 0.00005f;

    private readonly ParserService _parserService;

    public ParserTests(ITestOutputHelper outputHelper)
    {
        _parserService = new ParserService(Substitute.For<ILoggerFactory>());
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [Theory]
    [InlineData("//r	751	311	10	10	0	0	5s	100	1	101")]
    [InlineData("	//r	751	311	10	10	0	0	5s	100	1	101")]
    [InlineData("	")]
    [InlineData("")]
    [InlineData("//")]
    public void Spawn_Null(string input)
    {
        var result = _parserService.GetSpawnFromLine(input);

        result.Should().BeNull();
    }

    [Fact]
    public void Spawn_Regular()
    {
        const string input = "r	751	311	10	10	0	0	5s	100	1	101";
        var result = _parserService.GetSpawnFromLine(input);

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
        var result = _parserService.GetSpawnFromLine(input);

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
        var input = GetStreamReader("""
                                    Group	Test
                                    {
                                        Vnum	101
                                        Leader	Test	101
                                        1	Test	101
                                        2	Test	101
                                    }
                                    """);

        var groups = await _parserService.ParseFileGroups(input);

        var result = groups.Select(x => x.ToSpawnGroup()).FirstOrDefault();

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
        var input = GetStreamReader("""
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

        var groups = await _parserService.ParseFileGroups(input);

        var result = groups.Select(x => x.ToSpawnGroup()).FirstOrDefault();

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
        var input = GetStreamReader("""
                                    Group	GroupName
                                    {
                                    	Vnum	2430
                                    	Leader	Leader	2493
                                    	1	Mob1	2447
                                    	2	Mob2 2447
                                    	3	Mob 3	2513
                                    }
                                    """);

        var groups = await _parserService.ParseFileGroups(input);

        var result = groups.Select(x => x.ToSpawnGroup()).FirstOrDefault();

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
        var input = GetStreamReader("""
                                    Group	a1_01
                                    {
                                    	Vnum	101
                                    	1	101	1
                                    	2	171	1
                                    }
                                    """);

        var groups = await _parserService.ParseFileGroups(input);

        var result = groups.Select(x => x.ToSpawnGroupCollection()).FirstOrDefault();

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
        var input = GetStreamReader("""
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

        var groups = await _parserService.ParseFileGroups(input);

        var result = groups.Select(x => x.ToSpawnGroupCollection()).FirstOrDefault();

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
        var input = GetStreamReader("""
                                    Group	a1_01
                                    {
                                    	Vnum	101
                                    	1	101	1
                                    	2	171	1
                                    }
                                    			
                                    			
                                    """);

        var groups = await _parserService.ParseFileGroups(input);

        var result = groups.Select(x => x.ToSpawnGroupCollection()).FirstOrDefault();

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
        var input = GetStreamReader("""
                                    Group	a1_05
                                    {
                                    	Vnum	105
                                    			
                                    	1	112	1
                                    	2	113	1
                                    }
                                    """);

        var groups = await _parserService.ParseFileGroups(input);

        var result = groups.Select(x => x.ToSpawnGroupCollection()).FirstOrDefault();

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
        var input = GetStreamReader("""
                                    Group	a1_05
                                    {
                                    	Vnum	105
                                    	1	112
                                    }
                                    """);

        var groups = await _parserService.ParseFileGroups(input);

        var result = groups.Select(x => x.ToSpawnGroupCollection()).FirstOrDefault();

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
        var input = GetStreamReader("""
                                    			
                                    """);

        var groups = await _parserService.ParseFileGroups(input);

        var result = groups.Select(x => x.ToSpawnGroup()).FirstOrDefault();

        result.Should().BeNull();
    }

    private static StreamReader GetStreamReader(string input)
    {
        return new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input)));
    }

    [Fact]
    public async Task Drop_MobGroupItem_SingleDrop_SingleGroup()
    {
        var input = GetStreamReader("""
                                    Group	Abc
                                    {
                                    	Mob	101
                                    	Type	drop
                                    	1	10	1	0.09
                                    }
                                    """);

        var itemManager = Substitute.For<IItemManager>();

        var groups = await _parserService.ParseFileGroups(input);

        var mobDrops = groups.Select(x => _parserService.ParseMobGroup(x, itemManager)).ToList();

        groups.Should().HaveCount(1);
        groups[0].Should().BeEquivalentTo(new ParserService.DataFileGroup
        {
            Name = "Abc",
            Fields =
            {
                {"Mob", "101"},
                {"Type", "drop"}
            },
            Data =
            {
                new List<string>() {"1", "10", "1", "0.09"}
            }
        });
        mobDrops.Should().HaveCount(1);
        mobDrops[0].Should().NotBeNull();
        mobDrops[0].Should().BeEquivalentTo(new DropItemGroup
        {
            MonsterProtoId = 101,
            Drops = new List<DropItemGroup.Drop>
            {
                new()
                {
                    ItemProtoId = 10,
                    Amount = 1,
                    Chance = 0.09f * 10000.0f
                }
            }
        });
    }

    [Fact]
    public async Task Drop_MobGroupItem_SingleDrop_MultipleGroup()
    {
        var input = GetStreamReader("""
                                    Group	Abc
                                    {
                                    	Mob	101
                                    	Type	drop
                                    	1	10	1	0.09
                                    }
                                    Group	Def
                                    {
                                    	Mob	102
                                    	Type	drop
                                    	1	11	2	0.05
                                    }
                                    """);

        var itemManager = Substitute.For<IItemManager>();

        var groups = await _parserService.ParseFileGroups(input);

        var mobDrops = groups.Select(x => _parserService.ParseMobGroup(x, itemManager)).ToList();

        groups.Should().HaveCount(2);
        groups[0].Should().BeEquivalentTo(new ParserService.DataFileGroup
        {
            Name = "Abc",
            Fields =
            {
                {"Mob", "101"},
                {"Type", "drop"}
            },
            Data =
            {
                new List<string>() {"1", "10", "1", "0.09"}
            }
        });
        groups[1].Should().BeEquivalentTo(new ParserService.DataFileGroup
        {
            Name = "Def",
            Fields =
            {
                {"Mob", "102"},
                {"Type", "drop"}
            },
            Data =
            {
                new List<string>() {"1", "11", "2", "0.05"}
            }
        });

        mobDrops.Should().HaveCount(2);
        mobDrops[0].Should().NotBeNull();
        mobDrops[0].Should().BeEquivalentTo(new DropItemGroup
        {
            MonsterProtoId = 101,
            Drops = new List<DropItemGroup.Drop>
            {
                new()
                {
                    ItemProtoId = 10,
                    Amount = 1,
                    Chance = 0.09f * 10000.0f
                }
            }
        });
        mobDrops[1].Should().NotBeNull();
        mobDrops[1].Should().BeEquivalentTo(new DropItemGroup
        {
            MonsterProtoId = 102,
            Drops = new List<DropItemGroup.Drop>
            {
                new()
                {
                    ItemProtoId = 11,
                    Amount = 2,
                    Chance = 0.05f * 10000.0f
                }
            }
        });
    }

    [Fact]
    public async Task Drop_MobGroupItem_MultipleDrop_SingleGroup()
    {
        var input = GetStreamReader("""
                                    Group	Abc
                                    {
                                    	Mob	101
                                    	Type	drop
                                    	1	10	1	0.09
                                    	2	11	2	1
                                    }
                                    """);

        var itemManager = Substitute.For<IItemManager>();

        var groups = await _parserService.ParseFileGroups(input);

        var mobDrops = groups.Select(x => _parserService.ParseMobGroup(x, itemManager)).ToList();

        groups.Should().HaveCount(1);
        groups[0].Should().BeEquivalentTo(new ParserService.DataFileGroup
        {
            Name = "Abc",
            Fields =
            {
                {"Mob", "101"},
                {"Type", "drop"}
            },
            Data =
            {
                new List<string>() {"1", "10", "1", "0.09"},
                new List<string>() {"2", "11", "2", "1"}
            }
        });
        mobDrops.Should().HaveCount(1);
        mobDrops[0].Should().NotBeNull();
        mobDrops[0].Should().BeEquivalentTo(new DropItemGroup
        {
            MonsterProtoId = 101,
            Drops = new List<DropItemGroup.Drop>
            {
                new()
                {
                    ItemProtoId = 10,
                    Amount = 1,
                    Chance = 0.09f * 10000.0f
                },
                new()
                {
                    ItemProtoId = 11,
                    Amount = 2,
                    Chance = 1.0f * 10000.0f
                }
            }
        });
    }

    [Fact]
    public async Task Drop_MobGroupItem_MultipleDrop_MultipleGroup()
    {
        var input = GetStreamReader("""
                                    Group	Abc
                                    {
                                    	Mob	101
                                    	Type	drop
                                    	1	10	1	0.09
                                    	2	11	2	0.05
                                    }
                                    Group	Def
                                    {
                                    	Mob	102
                                    	Type	drop
                                    	1	11	2	0.05
                                    	2	10	1	0.09
                                    }
                                    """);

        var itemManager = Substitute.For<IItemManager>();

        var groups = await _parserService.ParseFileGroups(input);

        var mobDrops = groups.Select(x => _parserService.ParseMobGroup(x, itemManager)).ToList();

        groups.Should().HaveCount(2);
        groups[0].Should().BeEquivalentTo(new ParserService.DataFileGroup
        {
            Name = "Abc",
            Fields =
            {
                {"Mob", "101"},
                {"Type", "drop"}
            },
            Data =
            {
                new List<string>() {"1", "10", "1", "0.09"},
                new List<string>() {"2", "11", "2", "0.05"}
            }
        });
        groups[1].Should().BeEquivalentTo(new ParserService.DataFileGroup
        {
            Name = "Def",
            Fields =
            {
                {"Mob", "102"},
                {"Type", "drop"}
            },
            Data =
            {
                new List<string>() {"1", "11", "2", "0.05"},
                new List<string>() {"2", "10", "1", "0.09"}
            }
        });

        mobDrops.Should().HaveCount(2);
        mobDrops[0].Should().NotBeNull();
        mobDrops[0].Should().BeEquivalentTo(new DropItemGroup
        {
            MonsterProtoId = 101,
            Drops = new List<DropItemGroup.Drop>
            {
                new()
                {
                    ItemProtoId = 10,
                    Amount = 1,
                    Chance = 0.09f * 10000.0f
                },
                new()
                {
                    ItemProtoId = 11,
                    Amount = 2,
                    Chance = 0.05f * 10000.0f
                }
            }
        });
        mobDrops[1].Should().NotBeNull();
        mobDrops[1].Should().BeEquivalentTo(new DropItemGroup
        {
            MonsterProtoId = 102,
            Drops = new List<DropItemGroup.Drop>
            {
                new()
                {
                    ItemProtoId = 11,
                    Amount = 2,
                    Chance = 0.05f * 10000.0f
                },
                new()
                {
                    ItemProtoId = 10,
                    Amount = 1,
                    Chance = 0.09f * 10000.0f
                }
            }
        });
    }

    [Fact]
    public async Task Drop_LevelItem_SingleDrop_SingleGroup()
    {
        var input = GetStreamReader("""
                                    Group	Abc
                                    {
                                        Level_limit	75
                                    	Mob	101
                                    	Type	limit
                                    	1	10	1	0.09
                                    }
                                    """);

        var itemManager = Substitute.For<IItemManager>();

        var groups = await _parserService.ParseFileGroups(input);

        var mobDrops = groups.Select(x => _parserService.ParseMobGroup(x, itemManager)).ToList();

        groups.Should().HaveCount(1);
        groups[0].Should().BeEquivalentTo(new ParserService.DataFileGroup
        {
            Name = "Abc",
            Fields =
            {
                {"Level_limit", "75"},
                {"Mob", "101"},
                {"Type", "limit"}
            },
            Data =
            {
                new List<string>() {"1", "10", "1", "0.09"}
            }
        });
        mobDrops.Should().HaveCount(1);
        mobDrops[0].Should().NotBeNull();
        mobDrops[0].Should().BeEquivalentTo(new LevelItemGroup
        {
            LevelLimit = 75,
            Drops = new List<LevelItemGroup.Drop>
            {
                new()
                {
                    ItemProtoId = 10,
                    Amount = 1,
                    Chance = 0.09f * 10000.0f
                }
            }
        });
    }

    [Fact]
    public async Task Drop_LevelItem_SingleDrop_MultipleGroup()
    {
        var input = GetStreamReader("""
                                    Group	Abc
                                    {
                                        Level_limit	75
                                    	Mob	101
                                    	Type	limit
                                    	1	10	1	0.09
                                    }
                                    Group	Def
                                    {
                                        Level_limit	75
                                    	Mob	102
                                    	Type	limit
                                    	1	11	2	0.05
                                    }
                                    """);

        var itemManager = Substitute.For<IItemManager>();

        var groups = await _parserService.ParseFileGroups(input);

        var mobDrops = groups.Select(x => _parserService.ParseMobGroup(x, itemManager)).ToList();

        groups.Should().HaveCount(2);
        groups[0].Should().BeEquivalentTo(new ParserService.DataFileGroup
        {
            Name = "Abc",
            Fields =
            {
                {"Level_limit", "75"},
                {"Mob", "101"},
                {"Type", "limit"}
            },
            Data =
            {
                new List<string>() {"1", "10", "1", "0.09"}
            }
        });
        groups[1].Should().BeEquivalentTo(new ParserService.DataFileGroup
        {
            Name = "Def",
            Fields =
            {
                {"Level_limit", "75"},
                {"Mob", "102"},
                {"Type", "limit"}
            },
            Data =
            {
                new List<string>() {"1", "11", "2", "0.05"}
            }
        });
        mobDrops.Should().HaveCount(2);
        mobDrops[0].Should().NotBeNull();
        mobDrops[0].Should().BeEquivalentTo(new LevelItemGroup
        {
            LevelLimit = 75,
            Drops = new List<LevelItemGroup.Drop>
            {
                new()
                {
                    ItemProtoId = 10,
                    Amount = 1,
                    Chance = 0.09f * 10000.0f
                }
            }
        });
        mobDrops[1].Should().NotBeNull();
        mobDrops[1].Should().BeEquivalentTo(new LevelItemGroup
        {
            LevelLimit = 75,
            Drops = new List<LevelItemGroup.Drop>
            {
                new()
                {
                    ItemProtoId = 11,
                    Amount = 2,
                    Chance = 0.05f * 10000.0f
                }
            }
        });
    }

    [Fact]
    public async Task Drop_LevelItem_MultipleDrop_SingleGroup()
    {
        var input = GetStreamReader("""
                                    Group	Abc
                                    {
                                        Level_limit	75
                                    	Mob	101
                                    	Type	limit
                                    	1	10	1	0.09
                                    	2	11	2	1
                                    }
                                    """);

        var itemManager = Substitute.For<IItemManager>();

        var groups = await _parserService.ParseFileGroups(input);

        var mobDrops = groups.Select(x => _parserService.ParseMobGroup(x, itemManager)).ToList();

        groups.Should().HaveCount(1);
        groups[0].Should().BeEquivalentTo(new ParserService.DataFileGroup
        {
            Name = "Abc",
            Fields =
            {
                {"Level_limit", "75"},
                {"Mob", "101"},
                {"Type", "limit"}
            },
            Data =
            {
                new List<string>() {"1", "10", "1", "0.09"},
                new List<string>() {"2", "11", "2", "1"}
            }
        });
        mobDrops.Should().HaveCount(1);
        mobDrops[0].Should().NotBeNull();
        mobDrops[0].Should().BeEquivalentTo(new LevelItemGroup()
        {
            LevelLimit = 75,
            Drops = new List<LevelItemGroup.Drop>
            {
                new()
                {
                    ItemProtoId = 10,
                    Amount = 1,
                    Chance = 0.09f * 10000.0f
                },
                new()
                {
                    ItemProtoId = 11,
                    Amount = 2,
                    Chance = 1.0f * 10000.0f
                }
            }
        });
    }

    [Fact]
    public async Task Drop_LevelItem_MultipleDrop_MultipleGroup()
    {
        var input = GetStreamReader("""
                                    Group	Abc
                                    {
                                        Level_limit	75
                                    	Mob	101
                                    	Type	limit
                                    	1	10	1	0.09
                                    	2	11	2	1
                                    }
                                    Group	Def
                                    {
                                        Level_limit	75
                                    	Mob	102
                                    	Type	limit
                                    	1	11	2	0.05
                                    	2	10	1	0.09
                                    }
                                    """);

        var itemManager = Substitute.For<IItemManager>();

        var groups = await _parserService.ParseFileGroups(input);

        var mobDrops = groups.Select(x => _parserService.ParseMobGroup(x, itemManager)).ToList();

        groups.Should().HaveCount(2);
        groups[0].Should().BeEquivalentTo(new ParserService.DataFileGroup
        {
            Name = "Abc",
            Fields =
            {
                {"Level_limit", "75"},
                {"Mob", "101"},
                {"Type", "limit"}
            },
            Data =
            {
                new List<string>() {"1", "10", "1", "0.09"},
                new List<string>() {"2", "11", "2", "1"}
            }
        });
        groups[1].Should().BeEquivalentTo(new ParserService.DataFileGroup
        {
            Name = "Def",
            Fields =
            {
                {"Level_limit", "75"},
                {"Mob", "102"},
                {"Type", "limit"}
            },
            Data =
            {
                new List<string>() {"1", "11", "2", "0.05"},
                new List<string>() {"2", "10", "1", "0.09"}
            }
        });
        mobDrops.Should().HaveCount(2);
        mobDrops[0].Should().NotBeNull();
        mobDrops[0].Should().BeEquivalentTo(new LevelItemGroup()
        {
            LevelLimit = 75,
            Drops = new List<LevelItemGroup.Drop>
            {
                new()
                {
                    ItemProtoId = 10,
                    Amount = 1,
                    Chance = 0.09f * 10000.0f
                },
                new()
                {
                    ItemProtoId = 11,
                    Amount = 2,
                    Chance = 1.0f * 10000.0f
                }
            }
        });
        mobDrops[1].Should().NotBeNull();
        mobDrops[1].Should().BeEquivalentTo(new LevelItemGroup()
        {
            LevelLimit = 75,
            Drops = new List<LevelItemGroup.Drop>
            {
                new()
                {
                    ItemProtoId = 11,
                    Amount = 2,
                    Chance = 0.05f * 10000.0f
                },
                new()
                {
                    ItemProtoId = 10,
                    Amount = 1,
                    Chance = 0.09f * 10000.0f
                }
            }
        });
    }

    [Fact]
    public async Task Drop_MonsterItem_SingleDrop_SingleGroup()
    {
        var input = GetStreamReader("""
                                    Group	Abc
                                    {
                                        Kill_drop	75
                                    	Mob	101
                                    	Type	kill
                                    	1	10	1	20	30
                                    }
                                    """);

        var itemManager = Substitute.For<IItemManager>();

        var groups = await _parserService.ParseFileGroups(input);

        var mobDrops = groups.Select(x => _parserService.ParseMobGroup(x, itemManager)).ToList();

        groups.Should().HaveCount(1);
        groups[0].Should().BeEquivalentTo(new ParserService.DataFileGroup
        {
            Name = "Abc",
            Fields =
            {
                {"Kill_drop", "75"},
                {"Mob", "101"},
                {"Type", "kill"}
            },
            Data =
            {
                new List<string>() {"1", "10", "1", "20", "30"}
            }
        });
        mobDrops.Should().HaveCount(1);
        mobDrops[0].Should().NotBeNull();
        mobDrops[0].Should().BeEquivalentTo(new MonsterItemGroup()
        {
            MonsterProtoId = 101,
            MinKillCount = 75,
            Probabilities = new List<uint>() {20},
            Drops = new List<MonsterItemGroup.Drop>
            {
                new()
                {
                    ItemProtoId = 10,
                    Amount = 1,
                    Chance = 30
                }
            }
        });
    }

    [Fact]
    public async Task Drop_MonsterItem_SingleDrop_MultipleGroup()
    {
        var input = GetStreamReader("""
                                    Group	Abc
                                    {
                                        Kill_drop	75
                                    	Mob	101
                                    	Type	kill
                                    	1	10	1	20	30
                                    }
                                    Group	Def
                                    {
                                        Kill_drop	75
                                    	Mob	102
                                    	Type	kill
                                    	1	11	2	20	30
                                    }
                                    """);

        var itemManager = Substitute.For<IItemManager>();

        var groups = await _parserService.ParseFileGroups(input);

        var mobDrops = groups.Select(x => _parserService.ParseMobGroup(x, itemManager)).ToList();

        groups.Should().HaveCount(2);
        groups[0].Should().BeEquivalentTo(new ParserService.DataFileGroup
        {
            Name = "Abc",
            Fields =
            {
                {"Kill_drop", "75"},
                {"Mob", "101"},
                {"Type", "kill"}
            },
            Data =
            {
                new List<string>() {"1", "10", "1", "20", "30"}
            }
        });
        groups[1].Should().BeEquivalentTo(new ParserService.DataFileGroup
        {
            Name = "Def",
            Fields =
            {
                {"Kill_drop", "75"},
                {"Mob", "102"},
                {"Type", "kill"}
            },
            Data =
            {
                new List<string>() {"1", "11", "2", "20", "30"}
            }
        });
        mobDrops.Should().HaveCount(2);
        mobDrops[0].Should().NotBeNull();
        mobDrops[0].Should().BeEquivalentTo(new MonsterItemGroup()
        {
            MonsterProtoId = 101,
            MinKillCount = 75,
            Probabilities = new List<uint>() {20},
            Drops = new List<MonsterItemGroup.Drop>
            {
                new()
                {
                    ItemProtoId = 10,
                    Amount = 1,
                    Chance = 30
                }
            }
        });
        mobDrops[1].Should().NotBeNull();
        mobDrops[1].Should().BeEquivalentTo(new MonsterItemGroup()
        {
            MonsterProtoId = 102,
            MinKillCount = 75,
            Probabilities = new List<uint>() {20},
            Drops = new List<MonsterItemGroup.Drop>
            {
                new()
                {
                    ItemProtoId = 11,
                    Amount = 2,
                    Chance = 30
                }
            }
        });
    }

    [Fact]
    public async Task Drop_MonsterItem_MultipleDrop_SingleGroup()
    {
        var input = GetStreamReader("""
                                    Group	Abc
                                    {
                                        Kill_drop	75
                                    	Mob	101
                                    	Type	kill
                                    	1	10	1	20	30
                                    	2	11	2	25	35
                                    }
                                    """);

        var itemManager = Substitute.For<IItemManager>();

        var groups = await _parserService.ParseFileGroups(input);

        var mobDrops = groups.Select(x => _parserService.ParseMobGroup(x, itemManager)).ToList();

        groups.Should().HaveCount(1);
        groups[0].Should().BeEquivalentTo(new ParserService.DataFileGroup
        {
            Name = "Abc",
            Fields =
            {
                {"Kill_drop", "75"},
                {"Mob", "101"},
                {"Type", "kill"}
            },
            Data =
            {
                new List<string>() {"1", "10", "1", "20", "30"},
                new List<string>() {"2", "11", "2", "25", "35"}
            }
        });
        mobDrops.Should().HaveCount(1);
        mobDrops[0].Should().NotBeNull();
        mobDrops[0].Should().BeEquivalentTo(new MonsterItemGroup()
        {
            MonsterProtoId = 101,
            MinKillCount = 75,
            Probabilities = new List<uint>() {20, 25},
            Drops = new List<MonsterItemGroup.Drop>
            {
                new()
                {
                    ItemProtoId = 10,
                    Amount = 1,
                    Chance = 30
                },
                new()
                {
                    ItemProtoId = 11,
                    Amount = 2,
                    Chance = 35
                }
            }
        });
    }

    [Fact]
    public async Task Drop_MonsterItem_MultipleDrop_MultipleGroup()
    {
        var input = GetStreamReader("""
                                    Group	Abc
                                    {
                                        Kill_drop	75
                                    	Mob	101
                                    	Type	kill
                                    	1	10	1	20	30
                                    	2	11	2	25	35
                                    }
                                    Group	Def
                                    {
                                        Kill_drop	75
                                    	Mob	102
                                    	Type	kill
                                    	1	11	2	20	30
                                    	2	11	2	25	35
                                    }
                                    """);

        var itemManager = Substitute.For<IItemManager>();

        var groups = await _parserService.ParseFileGroups(input);

        var mobDrops = groups.Select(x => _parserService.ParseMobGroup(x, itemManager)).ToList();

        groups.Should().HaveCount(2);
        groups[0].Should().BeEquivalentTo(new ParserService.DataFileGroup
        {
            Name = "Abc",
            Fields =
            {
                {"Kill_drop", "75"},
                {"Mob", "101"},
                {"Type", "kill"}
            },
            Data =
            {
                new List<string>() {"1", "10", "1", "20", "30"},
                new List<string>() {"2", "11", "2", "25", "35"}
            }
        });
        groups[1].Should().BeEquivalentTo(new ParserService.DataFileGroup
        {
            Name = "Def",
            Fields =
            {
                {"Kill_drop", "75"},
                {"Mob", "102"},
                {"Type", "kill"}
            },
            Data =
            {
                new List<string>() {"1", "11", "2", "20", "30"},
                new List<string>() {"2", "11", "2", "25", "35"}
            }
        });
        mobDrops.Should().HaveCount(2);
        mobDrops[0].Should().NotBeNull();
        mobDrops[0].Should().BeEquivalentTo(new MonsterItemGroup()
        {
            MonsterProtoId = 101,
            MinKillCount = 75,
            Probabilities = new List<uint>() {20, 25},
            Drops = new List<MonsterItemGroup.Drop>
            {
                new()
                {
                    ItemProtoId = 10,
                    Amount = 1,
                    Chance = 30
                },
                new()
                {
                    ItemProtoId = 11,
                    Amount = 2,
                    Chance = 35
                }
            }
        });
        mobDrops[1].Should().NotBeNull();
        mobDrops[1].Should().BeEquivalentTo(new MonsterItemGroup()
        {
            MonsterProtoId = 102,
            MinKillCount = 75,
            Probabilities = new List<uint>() {20, 25},
            Drops = new List<MonsterItemGroup.Drop>
            {
                new()
                {
                    ItemProtoId = 11,
                    Amount = 2,
                    Chance = 30
                },
                new()
                {
                    ItemProtoId = 11,
                    Amount = 2,
                    Chance = 35
                }
            }
        });
    }

    [Fact]
    public void MobDropGroup_Kill_InvalidKillDrop()
    {
        var input = new ParserService.DataFileGroup
        {
            Name = "Abc",
            Fields =
            {
                {"Mob", "101"},
                {"Type", "Kill"},
                {"Kill_drop", "0"}
            },
            Data =
            {
                new List<string>() {"1", "10", "1", "0.09"},
                new List<string>() {"2", "11", "2", "0.05"}
            }
        };
        var itemManager = Substitute.For<IItemManager>();

        var result = _parserService.ParseMobGroup(input, itemManager);

        result.Should().BeNull();
    }

    [Fact]
    public void MobDropGroup_Kill_NoKillDrop()
    {
        var input = new ParserService.DataFileGroup
        {
            Name = "Abc",
            Fields =
            {
                {"Mob", "101"},
                {"Type", "Kill"},
            },
            Data =
            {
                new List<string>() {"1", "10", "1", "0.09"},
                new List<string>() {"2", "11", "2", "0.05"}
            }
        };
        var itemManager = Substitute.For<IItemManager>();

        var result = _parserService.ParseMobGroup(input, itemManager);

        result.Should().BeNull();
    }

    [Fact]
    public void MobDropGroup_Kill_NoMob()
    {
        var input = new ParserService.DataFileGroup
        {
            Name = "Abc",
            Fields =
            {
                {"Type", "Kill"},
                {"Kill_drop", "10"}
            },
            Data =
            {
                new List<string>() {"1", "10", "1", "0.09"},
                new List<string>() {"2", "11", "2", "0.05"}
            }
        };
        var itemManager = Substitute.For<IItemManager>();

        var action = new Action(() => { _parserService.ParseMobGroup(input, itemManager); });

        Assert.Throws<MissingRequiredFieldException>(action);
    }

    [Fact]
    public void MobDropGroup_Limit_NoLevel()
    {
        var input = new ParserService.DataFileGroup
        {
            Name = "Abc",
            Fields =
            {
                {"Mob", "101"},
                {"Type", "Limit"},
            },
            Data =
            {
                new List<string>() {"1", "10", "1", "0.09"},
                new List<string>() {"2", "11", "2", "0.05"}
            }
        };
        var itemManager = Substitute.For<IItemManager>();

        var action = new Action(() => { _parserService.ParseMobGroup(input, itemManager); });

        Assert.Throws<MissingRequiredFieldException>(action);
    }

    [Fact]
    public void MobDropGroup_NoType()
    {
        var input = new ParserService.DataFileGroup
        {
            Name = "Abc",
            Fields =
            {
                {"Mob", "101"},
                {"Kill_drop", "10"}
            },
            Data =
            {
                new List<string>() {"1", "10", "1", "0.09"},
                new List<string>() {"2", "11", "2", "0.05"}
            }
        };
        var itemManager = Substitute.For<IItemManager>();

        var action = new Action(() => { _parserService.ParseMobGroup(input, itemManager); });

        Assert.Throws<MissingRequiredFieldException>(action);
    }

    [Fact]
    public async Task CommonDrop_SingleLine_SingleDrop()
    {
        var input = new StringReader("""
                                     ABC	1	15	0.08	11	5000
                                     """);

        var result = await _parserService.GetCommonDropsAsync(input);

        result.Should().HaveCount(1);
        result[0].MinLevel.Should().Be(1);
        result[0].MaxLevel.Should().Be(15);
        result[0].ItemProtoId.Should().Be(11);
        result[0].Chance.Should().BeApproximately(0.08f * 10000, CHANCE_ALLOWED_APPROXIMATION);
    }

    [Fact]
    public async Task CommonDrop_SingleLineNoLabel_SingleDrop()
    {
        var input = new StringReader("""
                                     	1	15	0.08	11	5000
                                     """);

        var result = await _parserService.GetCommonDropsAsync(input);

        result.Should().HaveCount(1);
        result[0].MinLevel.Should().Be(1);
        result[0].MaxLevel.Should().Be(15);
        result[0].ItemProtoId.Should().Be(11);
        result[0].Chance.Should().BeApproximately(0.08f * 10000, CHANCE_ALLOWED_APPROXIMATION);
    }

    [Fact]
    public async Task CommonDrop_SingleLineInvalid_SingleDrop()
    {
        var input = new StringReader("""
                                     					
                                     """);

        var result = await _parserService.GetCommonDropsAsync(input);

        result.Should().HaveCount(0);
    }

    [Fact]
    public async Task CommonDrop_SingleLine_MultipleDrop_WithLabels()
    {
        var input = new StringReader("""
                                     ABC	1	15	0.08	11	5000	DEF	1	15	0.104	11	3846	GHI	1	15	0.12	11	3333	JKL	1	15	0.32	11	1250
                                     """);

        var result = await _parserService.GetCommonDropsAsync(input);

        result.Should().HaveCount(4);
        result[0].MinLevel.Should().Be(1);
        result[0].MaxLevel.Should().Be(15);
        result[0].ItemProtoId.Should().Be(11);
        result[0].Chance.Should().BeApproximately(0.08f * 10000, CHANCE_ALLOWED_APPROXIMATION);

        result[1].MinLevel.Should().Be(1);
        result[1].MaxLevel.Should().Be(15);
        result[1].ItemProtoId.Should().Be(11);
        result[1].Chance.Should().BeApproximately(0.104f * 10000, CHANCE_ALLOWED_APPROXIMATION);

        result[2].MinLevel.Should().Be(1);
        result[2].MaxLevel.Should().Be(15);
        result[2].ItemProtoId.Should().Be(11);
        result[2].Chance.Should().BeApproximately(0.12f * 10000, CHANCE_ALLOWED_APPROXIMATION);

        result[3].MinLevel.Should().Be(1);
        result[3].MaxLevel.Should().Be(15);
        result[3].ItemProtoId.Should().Be(11);
        result[3].Chance.Should().BeApproximately(0.32f * 10000, CHANCE_ALLOWED_APPROXIMATION);
    }

    [Fact]
    public async Task CommonDrop_SingleLine_MultipleDrop()
    {
        var input = new StringReader("""
                                     1	15	0.04	12	10000		1	15	0.052	12	7692		1	15	0.06	12	6666		1	15	0.16	12	2500
                                     """);

        var result = await _parserService.GetCommonDropsAsync(input);

        result.Should().HaveCount(4);
        result[0].MinLevel.Should().Be(1);
        result[0].MaxLevel.Should().Be(15);
        result[0].ItemProtoId.Should().Be(12);
        result[0].Chance.Should().BeApproximately(0.04f * 10000, CHANCE_ALLOWED_APPROXIMATION);

        result[1].MinLevel.Should().Be(1);
        result[1].MaxLevel.Should().Be(15);
        result[1].ItemProtoId.Should().Be(12);
        result[1].Chance.Should().BeApproximately(0.052f * 10000, CHANCE_ALLOWED_APPROXIMATION);

        result[2].MinLevel.Should().Be(1);
        result[2].MaxLevel.Should().Be(15);
        result[2].ItemProtoId.Should().Be(12);
        result[2].Chance.Should().BeApproximately(0.06f * 10000, CHANCE_ALLOWED_APPROXIMATION);

        result[3].MinLevel.Should().Be(1);
        result[3].MaxLevel.Should().Be(15);
        result[3].ItemProtoId.Should().Be(12);
        result[3].Chance.Should().BeApproximately(0.16f * 10000, CHANCE_ALLOWED_APPROXIMATION);
    }
}
