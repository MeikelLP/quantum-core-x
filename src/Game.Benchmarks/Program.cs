using BenchmarkDotNet.Running;
using Game.Benchmarks.Benchmarks;

// BenchmarkRunner.Run<PacketSerializer_Serialize>();
// BenchmarkRunner.Run<PacketSerializer_Deserialize>();
// BenchmarkRunner.Run<PacketSender>();
// BenchmarkRunner.Run<PacketReaderBenchmark>();
// BenchmarkRunner.Run<PacketPoolBenchmark>();

// await new PacketReader2Benchmark().Enumerate_TaskAsync();

// var benchmark = new PacketReaderBenchmark();
// benchmark.Packets = 10;
// benchmark.Setup();
// benchmark.Sync();

BenchmarkRunner.Run<PacketReaderBenchmark>();
// await new PacketReaderBenchmark{Iterations = 10_000}.Handler();
// await new PacketReaderBenchmark{Iterations = 10_000}.Handler2();
// new PacketPoolBenchmark().Context();
