using Microsoft.Extensions.DependencyInjection;
using QuantumCore.Game.World;
using QuantumCore.Game.World.Entities;
using QuantumCore.Models;

namespace QuantumCore.Game;

/// <summary>
/// Singleton which can create a player for a given connection
/// </summary>
public class PlayerFactory : IPlayerFactory
{
    private readonly IServiceProvider _provider;

    public PlayerFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public async Task<IPlayerEntity> CreatePlayerAsync(IGameConnection connection, PlayerData player)
    {
        var entity = ActivatorUtilities.CreateInstance<PlayerEntity>(_provider, [connection, player]);
        await entity.LoadAsync();

        return entity;
    }
}