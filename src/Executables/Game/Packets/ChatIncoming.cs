using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ClientToServerPacket(0x03, HasSequence = true)]
public readonly ref partial struct ChatIncoming
{
    public readonly ushort Size;
    public readonly ChatMessageTypes MessageType;
    public readonly string Message;

    public ChatIncoming(ChatMessageTypes messageType, string message)
    {
        Size = (ushort) message.Length;
        MessageType = messageType;
        Message = message;
    }
}