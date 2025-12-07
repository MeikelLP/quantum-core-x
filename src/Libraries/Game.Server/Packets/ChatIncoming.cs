using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x03, EDirection.Incoming, Sequence = true)]
    [PacketGenerator]
    public partial class ChatIncoming
    {
        [Field(0)] public ushort Size => (ushort) Message.Length;
        [Field(1)] public ChatMessageType MessageType { get; set; }
        public string Message { get; set; } = "";

        public override string ToString()
        {
            return base.ToString() + $" {Message}";
        }
    }
}