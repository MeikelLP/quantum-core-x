using Microsoft.Extensions.ObjectPool;
using QuantumCore.API;
using QuantumCore.Networking;

namespace QuantumCore;

public interface IPacketContextFactory
{
    object GetContext<TConnection>(PacketHeaderDefinition header, TConnection connection, IPacket packet)
        where TConnection : IConnection;

    void Return(PacketHeaderDefinition header, object context);
}

public class PacketContextFactory : IPacketContextFactory
{
    record CacheInfo(
        Action<object, IConnection> ConnectionSetter,
        Action<object, IPacket> PacketSetter,
        Func<object> ContextGetter,
        Action<object> ContextReturner);


    private readonly Dictionary<PacketHeaderDefinition, CacheInfo> _pools;

    public PacketContextFactory()
    {
        _pools = new Dictionary<PacketHeaderDefinition, CacheInfo>();
    }

    public object GetContext<TConnection>(PacketHeaderDefinition header, TConnection connection, IPacket packet)
        where TConnection : IConnection
    {
        if (!_pools.TryGetValue(header, out var tuple))
        {
            var contextType = typeof(PacketContext<,>).MakeGenericType([connection.GetType(), packet.GetType()]);
            var poolPolicy = Activator.CreateInstance(typeof(DefaultPooledObjectPolicy<>).MakeGenericType(contextType));
            var poolType = typeof(DefaultObjectPool<>).MakeGenericType(contextType);
            var pool = Activator.CreateInstance(poolType, [poolPolicy])!;
            var contextGetterMethod = poolType.GetMethod(nameof(ObjectPool<object>.Get))!;
            var contextReturnerMethod = poolType.GetMethod(nameof(ObjectPool<object>.Return))!;
            var connectionSetterProperty =
                contextType.GetProperty(nameof(PacketContext<IConnection, IPacket>.Connection))!;
            var packetSetterProperty = contextType.GetProperty(nameof(PacketContext<IConnection, IPacket>.Packet))!;

            void ConnectionSetter(object context, IConnection conn)
            {
                connectionSetterProperty.SetValue(context, conn);
            }

            void PacketSetter(object context, IPacket pkg)
            {
                packetSetterProperty.SetValue(context, pkg);
            }

            object ContextGetter()
            {
                return contextGetterMethod.Invoke(pool, [])!;
            }

            void ContextReturner(object context)
            {
                using var fixedSizeScope = new FixedSizeArray<object>(1);
                fixedSizeScope.Array[0] = context;
                contextReturnerMethod.Invoke(pool, fixedSizeScope.Array);
            }

            tuple = new CacheInfo(ConnectionSetter, PacketSetter, ContextGetter, ContextReturner);
            _pools.Add(header, tuple);
        }

        var context = tuple.ContextGetter.Invoke();

        tuple.ConnectionSetter.Invoke(context, connection);
        tuple.PacketSetter.Invoke(context, packet);

        return context;
    }

    public void Return(PacketHeaderDefinition header, object context)
    {
        if (_pools.TryGetValue(header, out var tuple))
        {
            tuple.ContextReturner.Invoke(context);
        }
    }
}