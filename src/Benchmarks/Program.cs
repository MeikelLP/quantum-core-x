using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using BinarySerialization;
using QuantumCore.Game;
using QuantumCore.Game.Packets;
using QuantumCore.Networking;

BenchmarkRunner.Run<NetworkBenchmarks>();

// new PacketCharacterInfoClass().Write_BinarySerializer(new MemoryStream());

public static class Helpers
{
    public static CharacterInfo DeserializeFromStream(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(NetworkingConstants.BufferSize);
        try
        {
            var __Vid = stream.ReadValueFromStream<UInt32>(buffer);
            var __Name = stream.ReadStringFromStream(buffer, (int)25);
            var __Parts = new[]
            {
                stream.ReadValueFromStream<UInt16>(buffer), stream.ReadValueFromStream<UInt16>(buffer),
                stream.ReadValueFromStream<UInt16>(buffer), stream.ReadValueFromStream<UInt16>(buffer)
            };
            var __Empire = stream.ReadValueFromStream<Byte>(buffer);
            var __GuildId = stream.ReadValueFromStream<UInt32>(buffer);
            var __Level = stream.ReadValueFromStream<UInt32>(buffer);
            var __RankPoints = stream.ReadValueFromStream<Int16>(buffer);
            var __PkMode = stream.ReadValueFromStream<Byte>(buffer);
            var __MountVnum = stream.ReadValueFromStream<UInt32>(buffer);
            var obj = new CharacterInfo
            {
                Vid = __Vid,
                Name = __Name,
                Parts = __Parts,
                Empire = __Empire,
                GuildId = __GuildId,
                Level = __Level,
                RankPoints = __RankPoints,
                PkMode = __PkMode,
                MountVnum = __MountVnum
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

public class PacketCharacterInfoClass
{
    private static readonly BinarySerializer bs = new BinarySerializer();

    [FieldOrder(0)] public uint Vid;

    [FieldLength(PlayerConstants.PLAYER_NAME_MAX_LENGTH)] [FieldOrder(1)]
    public string Name;

    [FieldOrder(2)] [FieldLength(4)] public ushort[] Parts;
    [FieldOrder(3)] public byte Empire;
    [FieldOrder(4)] public uint GuildId;
    [FieldOrder(5)] public uint Level;
    [FieldOrder(6)] public short RankPoints;
    [FieldOrder(7)] public byte PkMode;
    [FieldOrder(8)] public uint MountVnum;

    public static PacketCharacterInfo Read_BinarySerializer(Stream stream)
    {
        return bs.Deserialize<PacketCharacterInfo>(stream);
    }

    public static Task<PacketCharacterInfo> Read_BinarySerializerAsync(Stream stream)
    {
        return bs.DeserializeAsync<PacketCharacterInfo>(stream);
    }

    public void Write_BinarySerializer(Stream stream)
    {
        bs.Serialize(stream, this);
    }

    public Task Write_BinarySerializerAsync(Stream stream)
    {
        return bs.SerializeAsync(stream, this);
    }
}

public struct PacketCharacterInfo
{
    private static readonly BinarySerializer bs = new BinarySerializer();

    public uint Vid;
    public string Name;
    public ushort[] Parts;
    public byte Empire;
    public uint GuildId;
    public uint Level;
    public short RankPoints;
    public byte PkMode;
    public uint MountVnum;

    public static PacketCharacterInfo Read(BinaryReader stream)
    {
        var vid = stream.ReadUInt32();
        Span<byte> nameBytes = stackalloc byte[PlayerConstants.PLAYER_NAME_MAX_LENGTH];
        stream.BaseStream.ReadExactly(nameBytes);
        var name = Encoding.ASCII.GetString(nameBytes);
        var parts = new[] {stream.ReadUInt16(), stream.ReadUInt16(), stream.ReadUInt16(), stream.ReadUInt16()};
        var empire = stream.ReadByte();
        var guildId = stream.ReadUInt32();
        var level = stream.ReadUInt32();
        var rankPoints = stream.ReadInt16();
        var pkMode = stream.ReadByte();
        var mountVnum = stream.ReadUInt32();
        return new PacketCharacterInfo
        {
            Vid = vid,
            Name = name,
            Parts = parts,
            Empire = empire,
            GuildId = guildId,
            Level = level,
            RankPoints = rankPoints,
            PkMode = pkMode,
            MountVnum = mountVnum,
        };
    }

    public static PacketCharacterInfo Read(Stream stream)
    {
        var bytes = ArrayPool<byte>.Shared.Rent(4096);
        Span<byte> buffer = bytes;

        stream.ReadExactly(buffer[0..4]);
        var __Vid = BitConverter.ToUInt32(buffer);
        stream.ReadExactly(buffer[0..4]);
        var __Name = ((ReadOnlySpan<byte>)buffer[0..25]).ReadNullTerminatedString();

        stream.ReadExactly(buffer[0..2]);
        var __Parts__0 = BitConverter.ToUInt16(buffer[0..2]);
        stream.ReadExactly(buffer[0..2]);
        var __Parts__1 = BitConverter.ToUInt16(buffer[0..2]);
        stream.ReadExactly(buffer[0..2]);
        var __Parts__2 = BitConverter.ToUInt16(buffer[0..2]);
        stream.ReadExactly(buffer[0..2]);
        var __Parts__3 = BitConverter.ToUInt16(buffer[0..2]);
        var __Parts = new[] {__Parts__0, __Parts__1, __Parts__2, __Parts__3};
        stream.ReadExactly(buffer[0..1]);
        var __Empire = buffer[0];
        stream.ReadExactly(buffer[0..4]);
        var __GuildId = BitConverter.ToUInt32(buffer[0..4]);
        stream.ReadExactly(buffer[0..4]);
        var __Level = BitConverter.ToUInt32(buffer[0..4]);
        stream.ReadExactly(buffer[0..2]);
        var __RankPoints = BitConverter.ToInt16(buffer[0..2]);
        stream.ReadExactly(buffer[0..1]);
        var __PkMode = buffer[0];
        stream.ReadExactly(buffer[0..4]);
        var __MountVnum = BitConverter.ToUInt32(buffer[0..4]);
        ArrayPool<byte>.Shared.Return(bytes);
        return new PacketCharacterInfo
        {
            Vid = __Vid,
            Name = __Name,
            Parts = __Parts,
            Empire = __Empire,
            GuildId = __GuildId,
            Level = __Level,
            RankPoints = __RankPoints,
            PkMode = __PkMode,
            MountVnum = __MountVnum,
        };
    }

    public void Write(Stream stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(4096);
        buffer[0] = 0x88;
        buffer[1] = (byte)(this.Vid >> 0);
        buffer[2] = (byte)(this.Vid >> 8);
        buffer[3] = (byte)(this.Vid >> 16);
        buffer[4] = (byte)(this.Vid >> 24);
        buffer.WriteString(this.Name, 5, (int)25);
        buffer[30] = (byte)(this.Parts[0] >> 0);
        buffer[31] = (byte)(this.Parts[0] >> 8);
        buffer[32] = (byte)(this.Parts[1] >> 0);
        buffer[33] = (byte)(this.Parts[1] >> 8);
        buffer[34] = (byte)(this.Parts[2] >> 0);
        buffer[35] = (byte)(this.Parts[2] >> 8);
        buffer[36] = (byte)(this.Parts[3] >> 0);
        buffer[37] = (byte)(this.Parts[3] >> 8);
        buffer[38] = this.Empire;
        buffer[39] = (byte)(this.GuildId >> 0);
        buffer[40] = (byte)(this.GuildId >> 8);
        buffer[41] = (byte)(this.GuildId >> 16);
        buffer[42] = (byte)(this.GuildId >> 24);
        buffer[43] = (byte)(this.Level >> 0);
        buffer[44] = (byte)(this.Level >> 8);
        buffer[45] = (byte)(this.Level >> 16);
        buffer[46] = (byte)(this.Level >> 24);
        buffer[47] = (byte)(this.RankPoints >> 0);
        buffer[48] = (byte)(this.RankPoints >> 8);
        buffer[49] = this.PkMode;
        buffer[50] = (byte)(this.MountVnum >> 0);
        buffer[51] = (byte)(this.MountVnum >> 8);
        buffer[52] = (byte)(this.MountVnum >> 16);
        buffer[53] = (byte)(this.MountVnum >> 24);

        stream.Write(buffer, 0, 54);
        ArrayPool<byte>.Shared.Return(buffer);
    }

    public void Write(BinaryWriter stream)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(4096);
        stream.Write(0x88);
        stream.Write(Vid);
        Span<byte> strBytes = stackalloc byte[25];
        Encoding.ASCII.GetBytes(Name, strBytes);
        stream.Write(strBytes);
        stream.Write(Parts[0]);
        stream.Write(Parts[1]);
        stream.Write(Parts[2]);
        stream.Write(Parts[3]);
        stream.Write(GuildId);
        stream.Write(Level);
        stream.Write(RankPoints);
        stream.Write(PkMode);
        stream.Write(MountVnum);
        ArrayPool<byte>.Shared.Return(buffer);
    }
}

[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class NetworkBenchmarks
{
    private readonly BinaryReader _br;
    private readonly BinaryWriter _bw;
    private readonly MemoryStream _ms;

    private readonly CharacterInfo _oldPacket =
        new CharacterInfo {Vid = 1234, Name = "Meikel", Parts = [1234, 1234, 1234, 1234]};

    private readonly PacketCharacterInfo _newPacket =
        new PacketCharacterInfo {Vid = 1234, Name = "Meikel", Parts = [1234, 1234, 1234, 1234]};

    private readonly PacketCharacterInfoClass _newPacketClass =
        new PacketCharacterInfoClass {Vid = 1234, Name = "Meikel", Parts = [1234, 1234, 1234, 1234]};

    private readonly byte[] _buffer = new byte[4096];

    public NetworkBenchmarks()
    {
        _ms = new MemoryStream(RandomNumberGenerator.GetBytes(4096));
        _ms.Capacity = 4096;
        _ms.Position = 0;
        _br = new BinaryReader(_ms);
        _bw = new BinaryWriter(_ms);
    }

    [BenchmarkCategory("ClientToServer"), Benchmark(Baseline = true)]
    public void ClientToServer_Old_Async()
    {
        _ = CharacterInfo.DeserializeFromStreamAsync(_ms).Result;
        _ms.Position = 0;
    }

    [BenchmarkCategory("ClientToServer"), Benchmark]
    public void ClientToServer_Old_NonAsync()
    {
        _ = Helpers.DeserializeFromStream(_ms);
        _ms.Position = 0;
    }

    [BenchmarkCategory("ClientToServer"), Benchmark]
    public void ClientToServer_New()
    {
        _ = PacketCharacterInfo.Read(_ms);
        _ms.Position = 0;
    }

    [BenchmarkCategory("ClientToServer"), Benchmark]
    public void ClientToServer_NewBinary()
    {
        _ = PacketCharacterInfo.Read(_br);
        _ms.Position = 0;
    }

    [BenchmarkCategory("ClientToServer"), Benchmark]
    public void ClientToServer_BinarySerializer()
    {
        _ = PacketCharacterInfoClass.Read_BinarySerializer(_ms);
        _ms.Position = 0;
    }

    [BenchmarkCategory("ClientToServer"), Benchmark]
    public async Task ClientToServer_BinarySerializerAsync()
    {
        _ = await PacketCharacterInfoClass.Read_BinarySerializerAsync(_ms);
        _ms.Position = 0;
    }

    [BenchmarkCategory("ServerToClient"), Benchmark(Baseline = true)]
    public void ServerToClient_Old()
    {
        _oldPacket.Serialize(_buffer);
        _ms.Position = 0;
    }

    [BenchmarkCategory("ServerToClient"), Benchmark]
    public void ServerToClient_New()
    {
        _newPacket.Write(_ms);
        _ms.Position = 0;
    }

    [BenchmarkCategory("ServerToClient"), Benchmark]
    public void ServerToClient_NewBinary()
    {
        _newPacket.Write(_bw);
        _ms.Position = 0;
    }

    [BenchmarkCategory("ServerToClient"), Benchmark]
    public void ServerToClient_BinarySerializer()
    {
        _newPacketClass.Write_BinarySerializer(_ms);
        _ms.Position = 0;
    }

    [BenchmarkCategory("ServerToClient"), Benchmark]
    public async Task ServerToClient_BinarySerializerAsync()
    {
        await _newPacketClass.Write_BinarySerializerAsync(_ms);
        _ms.Position = 0;
    }
}
