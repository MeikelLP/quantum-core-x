using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ClientToServerPacket(0xff)]
public readonly ref partial struct GCHandshake
{
    public readonly uint Handshake;
    public readonly uint Time;
    public readonly uint Delta;
}