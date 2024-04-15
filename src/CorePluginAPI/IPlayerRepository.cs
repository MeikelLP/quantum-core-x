using System;
using System.Threading.Tasks;
using QuantumCore.API.Core.Models;

namespace QuantumCore.API;

#nullable enable

public interface IPlayerRepository
{
    Task<PlayerData?> GetPlayerAsync(Guid playerId);
}