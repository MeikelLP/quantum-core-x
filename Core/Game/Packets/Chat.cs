using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets
{
    enum ChatMessageTypes : byte
    {
        Normal,
        Info,
        // What is type 2?
        Group = 3,
        Guild,
        Command,
        Shout,
    };


    [Packet(0x03, EDirection.Incoming, Sequence = true)]
    public class ChatIncoming
    {
        [Field(0)]
        [Size]
        public ushort Size { get; set; }
        [Field(1)]
        public byte MessageType { get; set; }
        [Dynamic]
        public string Message { get; set; }

        public override string ToString()
        {
            return base.ToString() + $" {Message}";
        }
    }

    [Packet(0x04, EDirection.Outgoing, Sequence = false)]
    class ChatOutcoming
    {
        [Field(0)]
        [Size]
        public ushort Size { get; set; }

        [Field(1)]
        public byte MessageType { get; set; }

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
}