using System.Buffers;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Guild;

[Packet(0x4B, EDirection.Incoming)]
[SubPacket(0x05, 0)]
public partial class GuildNewsAddPacket
{
    [Field(0)] public byte Size => (byte) Value.Length;
    [Field(1, Length = 15)] public string Value { get; set; } = "";
}

// workaround because the packet serializer currently does not support GetSize with 2byte size or bigger
public partial class GuildNewsAddPacket : IPacketSerializable
{
    public static byte Header => 0x50;
    public static byte? SubHeader => 0x05;
    public static bool HasStaticSize => true;
    public static bool HasSequence => false;

    public void Serialize(byte[] bytes, in int offset = 0)
    {
        bytes[offset + 0] = 0x50;
        bytes[offset + 1] = 0x05;
        bytes[offset + 2] = (byte) (this.GetSize() >> 8);
        bytes[offset + 3] = (byte) (this.GetSize() >> 16);
        bytes.WriteString(this.Value, offset + 3, (int) this.Size + 1);
    }

    public ushort GetSize()
    {
        return 18;
    }

    public static GuildNewsAddPacket Deserialize(ReadOnlySpan<byte> bytes, in int offset = 0)
    {
        var __Size = bytes[(offset + 0)] - 17;
        var __Value = (bytes[(offset + 1)..(Index) (offset + 1 + __Size + 15)]).ReadNullTerminatedString();
        var obj = new GuildNewsAddPacket
        {
            Value = __Value
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
            var __Size = await stream.ReadValueFromStreamAsync<Byte>(buffer) - 17;
            var __Value = await stream.ReadStringFromStreamAsync(buffer, (int) 15);
            var obj = new GuildNewsAddPacket
            {
                Value = __Value
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