using System.Net;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;

namespace QuantumCore.API;

public interface IGameConnection : IConnection
{
    IServerBase Server { get; }
    IPAddress BoundIpAddress { get; }
    Guid? AccountId { get; set; }
    string Username { get; set; }
    IPlayerEntity? Player { get; set; }
    bool HandleHandshake(GcHandshakeData handshake);
}
