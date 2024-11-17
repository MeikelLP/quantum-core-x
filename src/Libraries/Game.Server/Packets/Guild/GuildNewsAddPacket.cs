using System.Buffers;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Guild;

[Packet(0x50, EDirection.Incoming)]
[SubPacket(0x05, 0)]
public partial class GuildNewsAddPacket
{
    [Field(0)] public byte Size => (byte) Message.Length;
    [Field(1)] public string Message { get; set; } = "";
}

// workaround because the packet serializer currently does not support GetSize with 2byte size or bigger
public partial class GuildNewsAddPacket : IPacketSerializable
{
    public static byte Header => 0x50;
    public static byte? SubHeader => 0x05;
    public static bool HasStaticSize => false;
    public static bool HasSequence => false;

    public void Serialize(byte[] bytes, in int offset = 0)
    {
        bytes[offset + 0] = 0x50;
        bytes[offset + 1] = 0x05;
        bytes[offset + 2] = (byte) this.GetSize();
        bytes.WriteString(this.Message, offset + 3, (int) this.Size + 1);
    }

    public ushort GetSize()
    {
        return (ushort) (3 + this.Message.Length + 1);
    }

    public static GuildNewsAddPacket Deserialize(ReadOnlySpan<byte> bytes, in int offset = 0)
    {
        var __Size = bytes[(offset + 0)] + 1;
        var __Message = (bytes[(offset + 1)..(Index) (offset + 1 + __Size)]).ReadNullTerminatedString();
        var obj = new GuildNewsAddPacket
        {
            Message = __Message
        };
        return obj;
    }

    public static T Deserialize<T>(ReadOnlySpan<byte> bytes, in int offset = 0)
        where T : IPacketSerializable
    {
        return (T) (object) Deserialize(bytes, offset);
    }

    public static async ValueTask<object> DeserializeFromStreamAsync(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(NetworkingConstants.BufferSize);
        try
        {
            var __Size = await stream.ReadValueFromStreamAsync<Byte>(buffer) + 1;
            var __Message = await stream.ReadStringFromStreamAsync(buffer, (int) __Size);
            var obj = new GuildNewsAddPacket
            {
                Message = __Message
            };
            return obj;
        }
        catch (Exception)
        {
            throw;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}