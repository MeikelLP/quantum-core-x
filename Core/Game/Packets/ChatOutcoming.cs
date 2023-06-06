using QuantumCore.Core.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x04, EDirection.Outgoing, Sequence = false)]
public class ChatOutcoming
{
    [Field(0)]
    [Size]
    public ushort Size { get; set; }

    [Field(1, EnumType = typeof(byte))]
    public ChatMessageTypes MessageType { get; set; }

    [Field(2)]
    public uint Vid { get; set; }

    [Field(3)]
    public byte Empire { get; set; }

    [Dynamic]
    public string Message { get; set; }

    public override string ToString()
    {
        return base.ToString() + $" {Message}";
    }
}