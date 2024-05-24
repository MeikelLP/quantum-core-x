using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ClientToServerPacket(0x0a, HasSequence = true)]
public readonly ref partial struct EnterGame
{
}