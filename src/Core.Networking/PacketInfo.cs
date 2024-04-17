namespace QuantumCore.Networking;

public readonly struct PacketInfo
{
    public readonly bool HasStaticSize;
    public readonly bool HasSequence;
    public readonly Type PacketType;
    public readonly Type? PacketHandlerType;

    public PacketInfo(Type packetType, Type? packetHandlerType, bool hasStaticSize, bool hasSequence)
    {
        PacketType = packetType;
        PacketHandlerType = packetHandlerType;
        HasStaticSize = hasStaticSize;
        HasSequence = hasSequence;
    }

    public IPacketSerializable Deserialize(ReadOnlySpan<byte> bytes)
    {
        var obj = (IPacketSerializable)Activator.CreateInstance(PacketType)!;
        obj.Deserialize(bytes);
        return obj;
    }

    public async ValueTask<IPacketSerializable> DeserializeFromStreamAsync(Stream stream)
    {
        var obj = (IPacketSerializable)Activator.CreateInstance(PacketType)!;
        await obj.DeserializeFromStreamAsync(stream);
        return obj;
    }
}
