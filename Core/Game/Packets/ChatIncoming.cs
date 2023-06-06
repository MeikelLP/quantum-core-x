using QuantumCore.Core.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x03, EDirection.Incoming, Sequence = true)]
    public class ChatIncoming
    {
        [Field(0)]
        [Size]
        public ushort Size { get; set; }
        [Field(1, EnumType = typeof(byte))]
        public ChatMessageTypes MessageType { get; set; }
        [Dynamic]
        public string Message { get; set; }

        public override string ToString()
        {
            return base.ToString() + $" {Message}";
        }
    }
}