namespace QuantumCore.Networking;

public class DefaultPacketSerializer : IPacketSerializer
{
    public byte[] Serialize<T>(T obj) where T : IPacketSerializable
    {
        var size = obj.GetSize();
        var arr = new byte[size];
        obj.Serialize(arr);
        return arr;
    }

    public void Serialize<T>(byte[] arr, T obj, int offset = 0)
        where T : IPacketSerializable
    {
        obj.Serialize(arr, offset);
    }

    public T Deserialize<T>(byte[] bytes, int offset = 0)
        where T : IPacketSerializable
    {
        return T.Deserialize<T>(bytes, offset);
    }
}