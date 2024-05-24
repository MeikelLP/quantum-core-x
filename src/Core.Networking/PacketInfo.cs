using System.Linq.Expressions;

namespace QuantumCore.Networking;

public readonly struct PacketInfo
{
    private delegate object DeserializeMethod(ReadOnlySpan<byte> bytes, int offset = 0);

    private delegate ValueTask<object> DeserializeFromStreamMethod(Stream stream);

    private readonly DeserializeMethod _deserializeDelegate;
    private readonly DeserializeFromStreamMethod _deserializeFromStreamMethod;
    public bool HasStaticSize { get; }
    public bool HasSequence { get; }
    public Type PacketType { get; }
    public Type? PacketHandlerType { get; }

    public PacketInfo(Type packetType, Type? packetHandlerType = null)
    {
        if (!packetType.IsAssignableTo(typeof(IPacketSerializable)))
            throw new ArgumentException($"Type must implement {nameof(IPacketSerializable)}", nameof(packetType));
        PacketType = packetType;
        PacketHandlerType = packetHandlerType;
        HasStaticSize = (bool) packetType.GetProperty(nameof(IPacketSerializable.HasStaticSize))!.GetValue(null)!;
        HasSequence = (bool) packetType.GetProperty(nameof(IPacketSerializable.HasSequence))!.GetValue(null)!;

        // Deserialize
        var bytesParam = Expression.Parameter(typeof(ReadOnlySpan<byte>));
        var offsetParam = Expression.Parameter(typeof(int));
        var methodCall = Expression.Call(packetType, "Deserialize", new[] {packetType}, bytesParam, offsetParam);
        _deserializeDelegate = Expression.Lambda<DeserializeMethod>(methodCall, bytesParam, offsetParam).Compile();

        var streamParam = Expression.Parameter(typeof(Stream));
        var deserializeFromStreamCall = Expression.Call(packetType,
            nameof(IPacketSerializable.DeserializeFromStreamAsync), Array.Empty<Type>(), streamParam);
        _deserializeFromStreamMethod =
            Expression.Lambda<DeserializeFromStreamMethod>(deserializeFromStreamCall, streamParam).Compile();
    }

    public object Deserialize(ReadOnlySpan<byte> bytes)
    {
        return _deserializeDelegate(bytes);
    }

    public ValueTask<object> DeserializeFromStreamAsync(Stream stream)
    {
        return _deserializeFromStreamMethod(stream);
    }
}