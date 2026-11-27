using System.Net;
using QuantumCore.Game.World;
using QuantumCore.Models;

namespace QuantumCore;

public interface IGameConnection : IConnection
{
    IServerBase Server { get; }
    IPAddress BoundIpAddress { get; }
    Guid? AccountId { get; set; }
    string Username { get; set; }
    IPlayerEntity? Player { get; set; }
    bool HandleHandshake(GcHandshakeData handshake);
}
