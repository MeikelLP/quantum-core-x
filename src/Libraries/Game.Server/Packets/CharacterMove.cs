using QuantumCore.API.Game.Types.Players;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x07, EDirection.Incoming, Sequence = true)]
[PacketGenerator]
public partial class CharacterMove
{
    [Field(0)] public CharacterMovementType MovementType { get; set; }
    [Field(1)] public byte Argument { get; set; }
    [Field(2)] public byte Rotation { get; set; }
    [Field(3)] public int PositionX { get; set; }
    [Field(4)] public int PositionY { get; set; }
    [Field(5)] public uint Time { get; set; }
}