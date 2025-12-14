using QuantumCore.API.Game.Types.Players;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x03, EDirection.Outgoing, Sequence = true)]
[PacketGenerator]
public partial class CharacterMoveOut
{
    [Field(0)] public CharacterMovementType MovementType { get; set; }
    [Field(1)] public byte Argument { get; set; }
    [Field(2)] public byte Rotation { get; set; }
    [Field(3)] public uint Vid { get; set; }
    [Field(4)] public int PositionX { get; set; }
    [Field(5)] public int PositionY { get; set; }
    [Field(6)] public uint Time { get; set; }
    [Field(7)] public uint Duration { get; set; }
}