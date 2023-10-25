using System;
using System.Threading.Tasks;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;

namespace QuantumCore.API;

public interface IGameConnection : IConnection
{
    IServerBase Server { get; }
    Guid? AccountId { get; set; }
    string Username { get; set; }
    IPlayerEntity Player { get; set; }
    bool HandleHandshake(GCHandshakeData handshake);
}
