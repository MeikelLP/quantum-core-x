using QuantumCore.Networking;

namespace QuantumCore.API.Packets;

[Packet(0x22, EDirection.OUTGOING)]
[PacketGenerator]
public partial class WhisperOutcoming
{
    [Field(0)] public ushort Size => (ushort)Message.Length;
    [Field(1)] public WhisperType Type { get; set; }

    [Field(2, Length = 25)] public string NameFrom { get; set; } = "";

    public string Message { get; set; } = "";
}