using AwesomeAssertions;
using Core.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using QuantumCore;
using QuantumCore.API;
using QuantumCore.API.Game.Guild;
using QuantumCore.Game;
using QuantumCore.Game.Persistence;
using QuantumCore.Game.Persistence.Extensions;

namespace Game.Tests;

public class GuildTests
{
    [Fact]
    public async Task AddExperience_CleanAsync()
    {
        var gm = await SetupGuildAsync();
        var guild = await gm.AddExperienceAsync(1, 600_000, TestContext.Current.CancellationToken);
        guild.Should().BeEquivalentTo(new GuildData
        {
            Level = 2,
            Experience = 0,
            MaxMemberCount = 34,
            Members =
            [
                new GuildMemberData
                {
                    Id = 1,
                    Name = "Admin",
                    Level = 99,
                    Class = 0,
                    IsLeader = true,
                    Rank = 1,
                    SpentExperience = 600_000
                }
            ]
        }, cfg => cfg
            .Including(x => x.Level)
            .Including(x => x.Experience)
            .Including(x => x.MaxMemberCount)
            .Including(x => x.Members));
    }

    [Fact]
    public async Task AddExperience_ExistingAsync()
    {
        var gm = await SetupGuildAsync();
        await gm.AddExperienceAsync(1, 300_000, TestContext.Current.CancellationToken);
        var guild = await gm.AddExperienceAsync(1, 600_000, TestContext.Current.CancellationToken);
        guild.Should().BeEquivalentTo(new GuildData
        {
            Level = 2,
            Experience = 300_000,
            MaxMemberCount = 34,
            Members =
            [
                new GuildMemberData
                {
                    Id = 1,
                    Name = "Admin",
                    Level = 99,
                    Class = 0,
                    IsLeader = true,
                    Rank = 1,
                    SpentExperience = 900_000
                }
            ]
        }, cfg => cfg
            .Including(x => x.Level)
            .Including(x => x.Experience)
            .Including(x => x.MaxMemberCount)
            .Including(x => x.Members));
    }

    [Fact]
    public async Task AddExperience_Existing_AddPerfectAsync()
    {
        var gm = await SetupGuildAsync();
        await gm.AddExperienceAsync(1, 300_000, TestContext.Current.CancellationToken);
        var guild = await gm.AddExperienceAsync(1, 300_000, TestContext.Current.CancellationToken);
        guild.Should().BeEquivalentTo(new GuildData
        {
            Level = 2,
            Experience = 0,
            MaxMemberCount = 34,
            Members =
            [
                new GuildMemberData
                {
                    Id = 1,
                    Name = "Admin",
                    Level = 99,
                    Class = 0,
                    IsLeader = true,
                    Rank = 1,
                    SpentExperience = 600_000
                }
            ]
        }, cfg => cfg
            .Including(x => x.Level)
            .Including(x => x.Experience)
            .Including(x => x.MaxMemberCount)
            .Including(x => x.Members));
    }

    private static async Task<IGuildManager> SetupGuildAsync()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddOptions<DatabaseOptions>(HostingOptions.MODE_GAME).Configure(x =>
        {
            x.Provider = DatabaseProvider.SQLITE;
            x.ConnectionString = "Data Source=test.db";
        });
        var guildExperienceManager = Substitute.For<IGuildExperienceManager>();
        guildExperienceManager.GetNeededExperience(1).Returns(600_000u);
        guildExperienceManager.GetNeededExperience(2).Returns(1_800_000u);
        guildExperienceManager.GetMaxPlayers(1).Returns((ushort)32);
        guildExperienceManager.GetMaxPlayers(2).Returns((ushort)34);
        guildExperienceManager.GetMaxPlayers(3).Returns((ushort)36);
        guildExperienceManager.MaxLevel.Returns((byte)3);
        var services = serviceCollection
            .AddLogging()
            .AddGameDatabase()
            .AddSingleton(Substitute.For<IHostEnvironment>())
            .AddSingleton<IConfiguration>(new ConfigurationBuilder().Build())
            .AddSingleton(guildExperienceManager)
            .AddSingleton<IGuildManager, GuildManager>()
            .BuildServiceProvider();

        var gm = services.GetRequiredService<IGuildManager>();
        var db = services.GetRequiredService<GameDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.MigrateAsync(); // contains admin user
        await gm.CreateGuildAsync("Testificate", 1);
        return gm;
    }
}