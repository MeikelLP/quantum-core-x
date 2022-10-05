using System;
using System.Threading.Tasks;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;

namespace QuantumCore.API;

public interface IGameConnection : IConnection
{
    IGameServer Server { get; }
    Guid? AccountId { get; set; }
    string Username { get; set; }
    IPlayerEntity Player { get; set; }
    Task<bool> HandleHandshake(GCHandshakeData handshake);
}