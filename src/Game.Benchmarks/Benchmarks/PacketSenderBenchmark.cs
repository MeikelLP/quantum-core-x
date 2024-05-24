// using System.Buffers;
// using System.Collections.Concurrent;
// using System.Runtime.InteropServices;
// using BenchmarkDotNet.Attributes;
// using QuantumCore.API.PluginTypes;
// using QuantumCore.Networking;
//
// namespace Game.Benchmarks.Benchmarks;
//
// [MemoryDiagnoser]
// public class PacketSenderBenchmark
// {
//     private readonly PacketSender_ObjectQueue _objectSender = new();
//     private readonly PacketSender_ByteArrayQueue _byteArraySender = new();
//     private const int PACKET_SIZE = 13;
//
//     [Params(1, 10, 100)]
//     public int Packets { get; set; }
//
//     class PacketSender_ObjectQueue
//     {
//         public Queue<IPacketSerializable> PacketsToSend { get; }
//
//         public PacketSender_ObjectQueue()
//         {
//             PacketsToSend = new Queue<IPacketSerializable>(100);
//         }
//
//         public void SendAllPacket()
//         {
//             while (PacketsToSend.TryDequeue(out var packet))
//             {
//                 var size = packet.GetSize();
//                 var bytes = ArrayPool<byte>.Shared.Rent(size);
//                 Array.Clear(bytes, 0, size);
//                 packet.Serialize(bytes);
//
//                 ArrayPool<byte>.Shared.Return(bytes);
//             }
//         }
//     }
//
//     class PacketSender_ByteArrayQueue
//     {
//         public Queue<byte[]> PacketsToSend { get; }
//
//         public PacketSender_ByteArrayQueue()
//         {
//             PacketsToSend = new Queue<byte[]>(100);
//         }
//
//         public void SendAllPacket()
//         {
//             while (PacketsToSend.TryDequeue(out var bytes))
//             {
//                 ArrayPool<byte>.Shared.Return(bytes);
//             }
//         }
//     }
//
//     [StructLayout(LayoutKind.Sequential)]
//     public readonly ref struct GCHandshake_ReadonlyRefStruct(uint handshake, uint time, uint delta)
//     {
//         public readonly uint Handshake = handshake;
//         public readonly uint Time = time;
//         public readonly uint Delta = delta;
//
//         public byte Header => 0xff;
//         public byte? SubHeader => null;
//         public bool HasStaticSize => true;
//         public bool HasSequence => false;
//
//         public void Serialize(byte[] bytes, in int offset = 0)
//         {
//             bytes[offset + 0] = 0xff;
//             bytes[offset + 1] = (System.Byte)(this.Handshake >> 0);
//             bytes[offset + 2] = (System.Byte)(this.Handshake >> 8);
//             bytes[offset + 3] = (System.Byte)(this.Handshake >> 16);
//             bytes[offset + 4] = (System.Byte)(this.Handshake >> 24);
//             bytes[offset + 5] = (System.Byte)(this.Time >> 0);
//             bytes[offset + 6] = (System.Byte)(this.Time >> 8);
//             bytes[offset + 7] = (System.Byte)(this.Time >> 16);
//             bytes[offset + 8] = (System.Byte)(this.Time >> 24);
//             bytes[offset + 9] = (System.Byte)(this.Delta >> 0);
//             bytes[offset + 10] = (System.Byte)(this.Delta >> 8);
//             bytes[offset + 11] = (System.Byte)(this.Delta >> 16);
//             bytes[offset + 12] = (System.Byte)(this.Delta >> 24);
//         }
//
//         public ushort GetSize()
//         {
//             return PACKET_SIZE;
//         }
//
//         public GCHandshake_ReadonlyRefStruct Deserialize(ReadOnlySpan<byte> bytes, in int offset = 0)
//         {
//             var __Handshake =
//                 System.BitConverter.ToUInt32(bytes[(System.Index)(offset + 0)..(System.Index)(offset + 0 + 4)]);
//             var __Time = System.BitConverter.ToUInt32(bytes[(System.Index)(offset + 4)..(System.Index)(offset + 4 + 4)]);
//             var __Delta = System.BitConverter.ToUInt32(bytes[(System.Index)(offset + 8)..(System.Index)(offset + 8 + 4)]);
//
//             return new GCHandshake_ReadonlyRefStruct(__Handshake, __Time, __Delta);
//         }
//     }
//
//     [StructLayout(LayoutKind.Sequential)]
//     public struct GCHandshake_Struct : IPacketSerializable
//     {
//         public uint Handshake;
//         public uint Time;
//         public uint Delta;
//
//         public GCHandshake_Struct(uint handshake, uint time, uint delta)
//         {
//             Handshake = handshake;
//             Time = time;
//             Delta = delta;
//         }
//
//         public byte Header => 0xff;
//         public byte? SubHeader => null;
//         public bool HasStaticSize => true;
//         public bool HasSequence => false;
//
//         public void Serialize(byte[] bytes, in int offset = 0)
//         {
//             bytes[offset + 0] = 0xff;
//             bytes[offset + 1] = (System.Byte)(this.Handshake >> 0);
//             bytes[offset + 2] = (System.Byte)(this.Handshake >> 8);
//             bytes[offset + 3] = (System.Byte)(this.Handshake >> 16);
//             bytes[offset + 4] = (System.Byte)(this.Handshake >> 24);
//             bytes[offset + 5] = (System.Byte)(this.Time >> 0);
//             bytes[offset + 6] = (System.Byte)(this.Time >> 8);
//             bytes[offset + 7] = (System.Byte)(this.Time >> 16);
//             bytes[offset + 8] = (System.Byte)(this.Time >> 24);
//             bytes[offset + 9] = (System.Byte)(this.Delta >> 0);
//             bytes[offset + 10] = (System.Byte)(this.Delta >> 8);
//             bytes[offset + 11] = (System.Byte)(this.Delta >> 16);
//             bytes[offset + 12] = (System.Byte)(this.Delta >> 24);
//         }
//
//         public ushort GetSize()
//         {
//             return PACKET_SIZE;
//         }
//
//         public void Deserialize(ReadOnlySpan<byte> bytes, in int offset = 0)
//         {
//             Handshake = System.BitConverter.ToUInt32(bytes[(System.Index)(offset + 0)..(System.Index)(offset + 0 + 4)]);
//             Time = System.BitConverter.ToUInt32(bytes[(System.Index)(offset + 4)..(System.Index)(offset + 4 + 4)]);
//             Delta = System.BitConverter.ToUInt32(bytes[(System.Index)(offset + 8)..(System.Index)(offset + 8 + 4)]);
//         }
//
//         public async ValueTask DeserializeFromStreamAsync(Stream stream)
//         {
//             var buffer = ArrayPool<byte>.Shared.Rent(NetworkingConstants.BufferSize);
//             try
//             {
//                 Handshake = await stream.ReadValueFromStreamAsync<UInt32>(buffer);
//                 Time = await stream.ReadValueFromStreamAsync<UInt32>(buffer);
//                 Delta = await stream.ReadValueFromStreamAsync<UInt32>(buffer);
//             }
//             catch (Exception)
//             {
//                 throw;
//             }
//             finally
//             {
//                 ArrayPool<byte>.Shared.Return(buffer);
//             }
//         }
//     }
//
//     [Benchmark]
//     public void ObjectQueue()
//     {
//         _objectSender.PacketsToSend.Clear(); // otherwise this method increases the queue too much
//         for (int i = 0; i < Packets; i++)
//         {
//             _objectSender.PacketsToSend.Enqueue(new GCHandshake_Struct(4, 4, 4));
//         }
//
//         _byteArraySender.SendAllPacket();
//     }
//
//     [Benchmark]
//     public void ByteArrayQueue()
//     {
//         _byteArraySender.PacketsToSend.Clear(); // otherwise this method increases the queue too much
//         for (int i = 0; i < Packets; i++)
//         {
//             var obj = new GCHandshake_ReadonlyRefStruct(4, 4, 4);
//             var bytes = ArrayPool<byte>.Shared.Rent(obj.GetSize());
//             obj.Serialize(bytes);
//             _byteArraySender.PacketsToSend.Enqueue(bytes);
//         }
//
//         _byteArraySender.SendAllPacket();
//     }
// }

