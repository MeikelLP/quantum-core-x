using QuantumCore.API.Game.Types;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x04, EDirection.Outgoing, Sequence = false)]
[PacketGenerator]
public partial class ChatOutcoming
{
    [Field(0)] public ushort Size => (ushort)Message.Length;

    [Field(1)] public ChatMessageType MessageType { get; set; }

    [Field(2)] public uint Vid { get; set; }

    [Field(3)] public EEmpire Empire { get; set; }

    public string Message { get; set; } = "";

    public override string ToString()
    {
        return base.ToString() + $" {Message}";
    }
}
