// using System.Collections.Generic;
// using System.IO;
// using System.Threading.Tasks;
// using BenchmarkDotNet.Attributes;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Logging.Abstractions;
// using QuantumCore.Networking;
//
// namespace Game.Benchmarks.Benchmarks;
//
// [MemoryDiagnoser]
// [ExceptionDiagnoser]
// public class PacketReader2Benchmark
// {
//     private readonly PacketReader2 _packerReader;
//     private readonly MemoryStream _memoryStream = new(Buffer);
//     private static readonly byte[] Buffer = [0x00, 0x10, 0x10, 0x10, 0x10];
//     private static readonly PacketManager2 PacketManager = new PacketManager2(NullLogger<PacketManager2>.Instance, [typeof(MyCustomPacket)], [typeof(MyCustomPacketHandler)]);
//
//     // [Params(1, 10, 100, 1000)]
//     public int Iterations { get; set; } = 1;
//
//     public PacketReader2Benchmark()
//     {
//         var handlers = new Dictionary<byte, MyCustomPacketHandler>
//         {
//             {0, new MyCustomPacketHandler()}
//         };
//         _packerReader = new PacketReader2(NullLogger<PacketReader2>.Instance, PacketManager, new ConfigurationBuilder().Build(), handlers);
//     }
//
//     [Benchmark]
//     public async Task Enumerate_TaskAsync()
//     {
//         for (int i = 0; i < Iterations; i++)
//         {
//             _memoryStream.Position = 0;
//             await _packerReader.EnumerateAsync(_memoryStream);
//         }
//     }
//
//     [Benchmark]
//     public void Enumerate_Sync()
//     {
//         for (int i = 0; i < Iterations; i++)
//         {
//             _memoryStream.Position = 0;
//             _packerReader.Enumerate(_memoryStream);
//         }
//     }
//
//     [Benchmark]
//     public void Parse()
//     {
//         for (int i = 0; i < Iterations; i++)
//         {
//             _packerReader.TryInvokeHandler(0x00, Buffer);
//         }
//     }
//
//     [Benchmark]
//     public void PacketManager_GetInfo()
//     {
//         for (int i = 0; i < Iterations; i++)
//         {
//             PacketManager.TryGetPacketInfo(0x00, null, out var packetInfo);
//         }
//     }
//
//     [Benchmark]
//     public void PacketManager_IsSubPacketDefinition()
//     {
//         for (int i = 0; i < Iterations; i++)
//         {
//             PacketManager.IsSubPacketDefinition(0x00);
//         }
//     }
// }

