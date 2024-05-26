using QuantumCore.API;
using QuantumCore.Networking;

namespace QuantumCore.Auth;

public interface IAuthPacketHandler<TPacket> : IPacketHandler<AuthConnection, AuthPacketContext<TPacket>>
    where TPacket : IPacket
{
}