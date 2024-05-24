using QuantumCore.API;
using QuantumCore.Networking;

namespace QuantumCore.Game;

public interface IGamePacketHandler<TPacket> : IPacketHandler<GameConnection, GamePacketContext<TPacket>>
    where TPacket : IPacket
{
}