using QuantumCore.Networking;

namespace QuantumCore.Auth.Packets;

[ServerToClientPacket(0xfb, HasSequence = true)]
[ClientToServerPacket(0xfb, HasSequence = true)]
public partial class KeyAgreement
{
    public ushort ValueLength { get; set; }
    public ushort DataLength { get; set; }
    [FixedSizeArray(4)] public byte[] Data { get; set; }
}