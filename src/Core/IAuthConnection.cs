using QuantumCore.Models;

namespace QuantumCore;

public interface IAuthConnection : IConnection
{
    bool HandleHandshake(GcHandshakeData handshake);
}
