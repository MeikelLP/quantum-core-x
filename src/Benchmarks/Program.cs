using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkRunner.Run<NetworkBenchmarks>();


public class ExampleServerToClientPacket
{
    public uint Id { get; set; }
    public static implicit operator ReadOnlySpan<byte>(ExampleServerToClientPacket example)
    {
        unsafe
        {
            return new ReadOnlySpan<byte>((void*)example.Id, 4)[..4];
        }
    }
}
public class ExampleClientToServerPacket
{
    public uint Id { get; set; }
    public static ExampleClientToServerPacket FromBytes(ReadOnlySpan<byte> bytes) => new ExampleClientToServerPacket
    {
        Id = BitConverter.ToUInt32(bytes[..4])
    };
}

[MemoryDiagnoser]
public class NetworkBenchmarks
{
    private readonly MemoryStream _ms;
    private readonly byte[] _buffer;

    public NetworkBenchmarks()
    {
        _ms = new MemoryStream([1, 2, 3, 4]);
        _ms.Capacity = 4;
        _ms.Position = 0;
        _buffer = new byte[4];
    }
    
    [Benchmark]
    public void ClientToServer()
    {
        var bytes = _ms.Read(_buffer);
        var result = ExampleClientToServerPacket.FromBytes(_buffer);
        _ms.Position = 0;
    }
    
    [Benchmark]
    public void ServerToClient()
    {
        _ms.Write(new ExampleServerToClientPacket());
        _ms.Position = 0;
    }
}
