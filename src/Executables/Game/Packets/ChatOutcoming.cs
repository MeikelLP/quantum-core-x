using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x04, HasSequence = false)]
public readonly ref partial struct ChatOutcoming
{
    public readonly ushort Size;
    public readonly ChatMessageTypes MessageType;
    public readonly uint Vid;
    public readonly byte Empire;
    public readonly string Message;

    public ChatOutcoming(ChatMessageTypes messageType, uint vid, byte empire, string message)
    {
        Size = (ushort) message.Length;
        MessageType = messageType;
        Vid = vid;
        Empire = empire;
        Message = message;
    }
}