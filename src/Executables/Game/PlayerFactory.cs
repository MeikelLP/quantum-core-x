using Microsoft.Extensions.DependencyInjection;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.World.Entities;

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
        await entity.Load();

        return entity;
    }
}