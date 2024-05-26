using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace Game.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class PacketSerializer_Serialize
{
    private readonly byte[] _bytes;

    [Params(1, 10, 100, 1000)] public int Iterations { get; set; }

    public PacketSerializer_Serialize()
    {
        _bytes = new byte[13];
    }

    [Benchmark]
    public void Class()
    {
        for (int i = 0; i < Iterations; i++)
        {
            var obj = new GCHandshake_Class(4, 4, 4);
            obj.Serialize(_bytes);
        }
    }

    [Benchmark]
    public void ClassProperties()
    {
        for (int i = 0; i < Iterations; i++)
        {
            var obj = new GCHandshake_ClassProperties
            {
                Handshake = 4,
                Delta = 4,
                Time = 4
            };
            obj.Serialize(_bytes);
        }
    }

    [Benchmark]
    public void Struct()
    {
        for (int i = 0; i < Iterations; i++)
        {
            var obj = new GCHandshake_Struct(4, 4, 4);
            obj.Serialize(_bytes);
        }
    }

    [Benchmark]
    public void ReadonlyStruct()
    {
        for (int i = 0; i < Iterations; i++)
        {
            var obj = new GCHandshake_ReadonlyStruct(4, 4, 4);
            obj.Serialize(_bytes);
        }
    }

    [Benchmark]
    public void ReadonlyRefStruct()
    {
        for (int i = 0; i < Iterations; i++)
        {
            var obj = new GCHandshake_ReadonlyRefStruct(4, 4, 4);
            obj.Serialize(_bytes);
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public readonly ref struct GCHandshake_ReadonlyRefStruct(uint handshake, uint time, uint delta)
    {
        public readonly uint Handshake = handshake;
        public readonly uint Time = time;
        public readonly uint Delta = delta;

        public byte Header => 0xff;
        public byte? SubHeader => null;
        public bool HasStaticSize => true;
        public bool HasSequence => false;

        public void Serialize(byte[] bytes, in int offset = 0)
        {
            bytes[offset + 0] = 0xff;
            bytes[offset + 1] = (System.Byte) (this.Handshake >> 0);
            bytes[offset + 2] = (System.Byte) (this.Handshake >> 8);
            bytes[offset + 3] = (System.Byte) (this.Handshake >> 16);
            bytes[offset + 4] = (System.Byte) (this.Handshake >> 24);
            bytes[offset + 5] = (System.Byte) (this.Time >> 0);
            bytes[offset + 6] = (System.Byte) (this.Time >> 8);
            bytes[offset + 7] = (System.Byte) (this.Time >> 16);
            bytes[offset + 8] = (System.Byte) (this.Time >> 24);
            bytes[offset + 9] = (System.Byte) (this.Delta >> 0);
            bytes[offset + 10] = (System.Byte) (this.Delta >> 8);
            bytes[offset + 11] = (System.Byte) (this.Delta >> 16);
            bytes[offset + 12] = (System.Byte) (this.Delta >> 24);
        }

        public ushort GetSize()
        {
            return 13;
        }

        public GCHandshake_ReadonlyRefStruct Deserialize(ReadOnlySpan<byte> bytes, in int offset = 0)
        {
            var __Handshake =
                System.BitConverter.ToUInt32(bytes[(System.Index) (offset + 0)..(System.Index) (offset + 0 + 4)]);
            var __Time =
                System.BitConverter.ToUInt32(bytes[(System.Index) (offset + 4)..(System.Index) (offset + 4 + 4)]);
            var __Delta =
                System.BitConverter.ToUInt32(bytes[(System.Index) (offset + 8)..(System.Index) (offset + 8 + 4)]);

            return new GCHandshake_ReadonlyRefStruct(__Handshake, __Time, __Delta);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct GCHandshake_ReadonlyStruct(uint handshake, uint time, uint delta)
    {
        public readonly uint Handshake = handshake;
        public readonly uint Time = time;
        public readonly uint Delta = delta;

        public byte Header => 0xff;
        public byte? SubHeader => null;
        public bool HasStaticSize => true;
        public bool HasSequence => false;

        public void Serialize(byte[] bytes, in int offset = 0)
        {
            bytes[offset + 0] = 0xff;
            bytes[offset + 1] = (System.Byte) (this.Handshake >> 0);
            bytes[offset + 2] = (System.Byte) (this.Handshake >> 8);
            bytes[offset + 3] = (System.Byte) (this.Handshake >> 16);
            bytes[offset + 4] = (System.Byte) (this.Handshake >> 24);
            bytes[offset + 5] = (System.Byte) (this.Time >> 0);
            bytes[offset + 6] = (System.Byte) (this.Time >> 8);
            bytes[offset + 7] = (System.Byte) (this.Time >> 16);
            bytes[offset + 8] = (System.Byte) (this.Time >> 24);
            bytes[offset + 9] = (System.Byte) (this.Delta >> 0);
            bytes[offset + 10] = (System.Byte) (this.Delta >> 8);
            bytes[offset + 11] = (System.Byte) (this.Delta >> 16);
            bytes[offset + 12] = (System.Byte) (this.Delta >> 24);
        }

        public ushort GetSize()
        {
            return 13;
        }

        public GCHandshake_ReadonlyRefStruct Deserialize(ReadOnlySpan<byte> bytes, in int offset = 0)
        {
            var __Handshake =
                System.BitConverter.ToUInt32(bytes[(System.Index) (offset + 0)..(System.Index) (offset + 0 + 4)]);
            var __Time =
                System.BitConverter.ToUInt32(bytes[(System.Index) (offset + 4)..(System.Index) (offset + 4 + 4)]);
            var __Delta =
                System.BitConverter.ToUInt32(bytes[(System.Index) (offset + 8)..(System.Index) (offset + 8 + 4)]);

            return new GCHandshake_ReadonlyRefStruct(__Handshake, __Time, __Delta);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GCHandshake_Struct(uint handshake, uint time, uint delta)
    {
        public uint Handshake = handshake;
        public uint Time = time;
        public uint Delta = delta;

        public byte Header => 0xff;
        public byte? SubHeader => null;
        public bool HasStaticSize => true;
        public bool HasSequence => false;

        public void Serialize(byte[] bytes, in int offset = 0)
        {
            bytes[offset + 0] = 0xff;
            bytes[offset + 1] = (System.Byte) (this.Handshake >> 0);
            bytes[offset + 2] = (System.Byte) (this.Handshake >> 8);
            bytes[offset + 3] = (System.Byte) (this.Handshake >> 16);
            bytes[offset + 4] = (System.Byte) (this.Handshake >> 24);
            bytes[offset + 5] = (System.Byte) (this.Time >> 0);
            bytes[offset + 6] = (System.Byte) (this.Time >> 8);
            bytes[offset + 7] = (System.Byte) (this.Time >> 16);
            bytes[offset + 8] = (System.Byte) (this.Time >> 24);
            bytes[offset + 9] = (System.Byte) (this.Delta >> 0);
            bytes[offset + 10] = (System.Byte) (this.Delta >> 8);
            bytes[offset + 11] = (System.Byte) (this.Delta >> 16);
            bytes[offset + 12] = (System.Byte) (this.Delta >> 24);
        }

        public ushort GetSize()
        {
            return 13;
        }

        public void Deserialize(ReadOnlySpan<byte> bytes, in int offset = 0)
        {
            Handshake =
                System.BitConverter.ToUInt32(bytes[(System.Index) (offset + 0)..(System.Index) (offset + 0 + 4)]);
            Time = System.BitConverter.ToUInt32(bytes[(System.Index) (offset + 4)..(System.Index) (offset + 4 + 4)]);
            Delta = System.BitConverter.ToUInt32(bytes[(System.Index) (offset + 8)..(System.Index) (offset + 8 + 4)]);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GCHandshake_Class(uint handshake, uint time, uint delta)
    {
        public uint Handshake = handshake;
        public uint Time = time;
        public uint Delta = delta;

        public byte Header => 0xff;
        public byte? SubHeader => null;
        public bool HasStaticSize => true;
        public bool HasSequence => false;

        public void Serialize(byte[] bytes, in int offset = 0)
        {
            bytes[offset + 0] = 0xff;
            bytes[offset + 1] = (System.Byte) (this.Handshake >> 0);
            bytes[offset + 2] = (System.Byte) (this.Handshake >> 8);
            bytes[offset + 3] = (System.Byte) (this.Handshake >> 16);
            bytes[offset + 4] = (System.Byte) (this.Handshake >> 24);
            bytes[offset + 5] = (System.Byte) (this.Time >> 0);
            bytes[offset + 6] = (System.Byte) (this.Time >> 8);
            bytes[offset + 7] = (System.Byte) (this.Time >> 16);
            bytes[offset + 8] = (System.Byte) (this.Time >> 24);
            bytes[offset + 9] = (System.Byte) (this.Delta >> 0);
            bytes[offset + 10] = (System.Byte) (this.Delta >> 8);
            bytes[offset + 11] = (System.Byte) (this.Delta >> 16);
            bytes[offset + 12] = (System.Byte) (this.Delta >> 24);
        }

        public ushort GetSize()
        {
            return 13;
        }

        public void Deserialize(ReadOnlySpan<byte> bytes, in int offset = 0)
        {
            Handshake =
                System.BitConverter.ToUInt32(bytes[(System.Index) (offset + 0)..(System.Index) (offset + 0 + 4)]);
            Time = System.BitConverter.ToUInt32(bytes[(System.Index) (offset + 4)..(System.Index) (offset + 4 + 4)]);
            Delta = System.BitConverter.ToUInt32(bytes[(System.Index) (offset + 8)..(System.Index) (offset + 8 + 4)]);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class GCHandshake_ClassProperties
    {
        public uint Handshake { get; set; }
        public uint Time { get; set; }
        public uint Delta { get; set; }

        public byte Header => 0xff;
        public byte? SubHeader => null;
        public bool HasStaticSize => true;
        public bool HasSequence => false;

        public void Serialize(byte[] bytes, in int offset = 0)
        {
            bytes[offset + 0] = 0xff;
            bytes[offset + 1] = (System.Byte) (this.Handshake >> 0);
            bytes[offset + 2] = (System.Byte) (this.Handshake >> 8);
            bytes[offset + 3] = (System.Byte) (this.Handshake >> 16);
            bytes[offset + 4] = (System.Byte) (this.Handshake >> 24);
            bytes[offset + 5] = (System.Byte) (this.Time >> 0);
            bytes[offset + 6] = (System.Byte) (this.Time >> 8);
            bytes[offset + 7] = (System.Byte) (this.Time >> 16);
            bytes[offset + 8] = (System.Byte) (this.Time >> 24);
            bytes[offset + 9] = (System.Byte) (this.Delta >> 0);
            bytes[offset + 10] = (System.Byte) (this.Delta >> 8);
            bytes[offset + 11] = (System.Byte) (this.Delta >> 16);
            bytes[offset + 12] = (System.Byte) (this.Delta >> 24);
        }

        public ushort GetSize()
        {
            return 13;
        }

        public void Deserialize(ReadOnlySpan<byte> bytes, in int offset = 0)
        {
            Handshake =
                System.BitConverter.ToUInt32(bytes[(System.Index) (offset + 0)..(System.Index) (offset + 0 + 4)]);
            Time = System.BitConverter.ToUInt32(bytes[(System.Index) (offset + 4)..(System.Index) (offset + 4 + 4)]);
            Delta = System.BitConverter.ToUInt32(bytes[(System.Index) (offset + 8)..(System.Index) (offset + 8 + 4)]);
        }
    }
}