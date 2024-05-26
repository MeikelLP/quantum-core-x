using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using QuantumCore.API;
using QuantumCore.Networking;

namespace QuantumCore;

public class PacketReader2<TConnection> : IPacketReader2
    where TConnection : IConnection
{
    private readonly ILogger<PacketReader2<TConnection>> _logger;
    private readonly IPacketContextFactory _packetContextFactory;
    private readonly Dictionary<PacketHeaderDefinition, object> _packetPools;
    private readonly Dictionary<PacketHeaderDefinition, MethodInfo> _poolGetters;
    private readonly Dictionary<PacketHeaderDefinition, MethodInfo> _poolReturners;
    private readonly Dictionary<PacketHeaderDefinition, MethodInfo> _handlerInvokers;
    private readonly Dictionary<PacketHeaderDefinition, PacketInfo2> _packetInfos;
    private readonly Dictionary<PacketHeaderDefinition, Type> _handlerTypes;

    public PacketReader2(IEnumerable<PacketInfo2> packetInfos,
        ILogger<PacketReader2<TConnection>> logger, IPacketContextFactory packetContextFactory)
    {
        _logger = logger;
        _packetContextFactory = packetContextFactory;
        var arr = packetInfos as PacketInfo2[] ?? packetInfos.ToArray();
        _packetInfos = arr.ToDictionary(x => x.Header, x => x);
        _handlerInvokers = arr
            .Where(x => x.HandlerType is not null)
            .ToDictionary(x => x.Header,
                x =>
                {
                    return x.HandlerType!.GetMethod(nameof(IPacketHandler<TConnection, IPacket>.ExecuteAsync),
                        BindingFlags.Public | BindingFlags.Instance)!;
                });
        _packetPools = arr.ToDictionary(x => x.Header,
            x => Activator.CreateInstance(typeof(DefaultObjectPool<>).MakeGenericType(x.PacketType), [
                Activator.CreateInstance(typeof(DefaultPooledObjectPolicy<>).MakeGenericType(x.PacketType))
            ])!);
        _poolGetters = arr.ToDictionary(x => x.Header, x =>
        {
            return typeof(DefaultObjectPool<>)
                .MakeGenericType(x.PacketType)
                .GetMethod(nameof(DefaultObjectPool<object>.Get), BindingFlags.Public | BindingFlags.Instance)!;
        });
        _handlerTypes = arr
            .Where(x => x.HandlerType is not null)
            .ToDictionary(x => x.Header, x =>
            {
                return x.HandlerType!;
            });
        _poolReturners = arr.ToDictionary(x => x.Header, x =>
        {
            return typeof(DefaultObjectPool<>)
                .MakeGenericType(x.PacketType)
                .GetMethod(nameof(DefaultObjectPool<object>.Return), BindingFlags.Public | BindingFlags.Instance)!;
        });
    }

    public bool TryGetPacket(in PacketHeaderDefinition header, in ReadOnlySpan<byte> buffer,
        [MaybeNullWhen(false)] out IPacket packet)
    {
        if (!_packetPools.TryGetValue(header, out var pool))
        {
            _logger.LogError("Failed to create packet for {Header}", header);
            packet = null;
            return false;
        }

        var getter = _poolGetters[header];
        packet = (IPacket) getter.Invoke(pool, [])!;
        packet.Deserialize(buffer);

        return true;
    }

    public async ValueTask HandlePacketAsync(IServiceProvider scopedServiceProvider, PacketHeaderDefinition header,
        IPacket packet, CancellationToken token = default)
    {
        if (!_packetPools.TryGetValue(header, out var pool) ||
            !_handlerTypes.TryGetValue(header, out var handlerType) ||
            !_handlerInvokers.TryGetValue(header, out var handlerInvoker))
        {
            _logger.LogError("Failed to find packet handler for {Header}", header);
            return;
        }

        var context =
            _packetContextFactory.GetContext(header, scopedServiceProvider.GetRequiredService<TConnection>(), packet);
        var handler = scopedServiceProvider.GetRequiredService(handlerType);

        try
        {
            await (ValueTask) handlerInvoker.Invoke(handler, [context, token])!;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to execute packet handler");
        }
        finally
        {
            ReturnContext(header, packet, context, pool);
        }
    }

    private void ReturnContext(PacketHeaderDefinition header, IPacket packet, object context, object pool)
    {
        var returner = _poolReturners[header];
        _packetContextFactory.Return(header, context);
        using var fixedSizeArray = new FixedSizeArray<object>(1);
        fixedSizeArray.Array[0] = packet;
        returner.Invoke(pool, fixedSizeArray.Array);
    }

    public bool TryGetPacketInfo(in PacketHeaderDefinition header, [MaybeNullWhen(false)] out PacketInfo2 packet)
    {
        return _packetInfos.TryGetValue(header, out packet);
    }

    public bool IsSubPacketDefinition(in byte header)
    {
        return _packetInfos.TryGetValue(header, out var info) && info.Header.SubHeader is not null;
    }
}
