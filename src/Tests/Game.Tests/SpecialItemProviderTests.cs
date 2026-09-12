using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using NSubstitute;
using QuantumCore.API;
using QuantumCore.Game.Services;

namespace Game.Tests;

public class SpecialItemProviderTests
{
    private readonly ISpecialItemProvider _provider;

    public SpecialItemProviderTests()
    {
        _provider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<ISpecialItemProvider, SpecialItemProvider>()
            .AddSingleton(_ =>
            {
                var mock = Substitute.For<IFileProvider>();
                mock.GetFileInfo(Arg.Any<string>()).ReturnsForAnyArgs(call =>
                    new PhysicalFileInfo(new FileInfo(Path.Combine("Fixtures", call.Arg<string>()!))));
                return mock;
            })
            .BuildServiceProvider()
            .GetRequiredService<ISpecialItemProvider>();
    }

    private async Task LoadAsync()
    {
        await ((ILoadable)_provider).LoadAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ReadsTheContentsOfAContainerAsync()
    {
        await LoadAsync();

        _provider.GetPossibleContents(9001).Should().BeEquivalentTo([
            new SpecialItemEntry(5001, 1, 70),
            new SpecialItemEntry(5002, 3, 30)
        ]);
    }

    [Fact]
    public async Task IgnoresAttributeGroupsAsync()
    {
        await LoadAsync();

        // ATTR groups list random bonuses rather than items, so they yield no rewards
        _provider.GetPossibleContents(9003).Should().BeEmpty();
    }

    [Fact]
    public async Task ReportsNothingForAnItemThatIsNoContainerAsync()
    {
        await LoadAsync();

        _provider.GetPossibleContents(1234).Should().BeEmpty();
        _provider.Roll(1234).Should().BeNull();
    }

    [Fact]
    public async Task AlwaysRollsAnEntryOfTheContainerAsync()
    {
        await LoadAsync();

        var allowed = _provider.GetPossibleContents(9001);

        // the pick is random, so try often enough to cover every branch
        for (var i = 0; i < 100; i++)
        {
            _provider.Roll(9001).Should().NotBeNull().And.BeOneOf([.. allowed.Cast<SpecialItemEntry?>()]);
        }
    }
}
