using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using QuantumCore.Extensions;

namespace QuantumCore.Core.Networking;

public class DefaultPacketManager : IPacketManager
{
    private readonly ILogger<DefaultPacketManager> _logger;
    private readonly List<Type> _incomingTypes = new();
    private readonly List<Type> _outgoingTypes = new();
    public Dictionary<ushort, PacketCache> OutgoingPackets {get;} = new();
    public Dictionary<ushort, PacketCache> IncomingPackets {get;} = new();
    internal Dictionary<Type, SubPacketCache> SubTypes { get; } = new();

        

    public IPacketCache GetPacket<T>()
    {
        return GetPacket(typeof(T));
    }

    public IPacketCache GetPacket(Type type)
    {
        var attr = type.GetCustomAttribute<PacketAttribute>();
        if (attr is null)
        {
            if (!SubTypes.TryGetValue(type, out var subCache))
            {
                throw new Exception("TODO"); // TODO
            }

            return subCache;
        }
        var dir = attr.Direction;
        var header = attr.Header;

        if (dir is EDirection.Incoming)
        {
            return new PacketCache(header, IncomingPackets[header].Type);
        }
        else if (dir is EDirection.Outgoing)
        {
            return new PacketCache(header, OutgoingPackets[header].Type);
        }
        else
        {
            throw new InvalidOperationException($"Invalid direction {dir}");
        }
    }

    public DefaultPacketManager(ILogger<DefaultPacketManager> logger)
    {
        _logger = logger;
    }
    
    public bool IsRegisteredOutgoing(Type packet)
    {
        return _outgoingTypes.Contains(packet);
    }

    public PacketCache GetOutgoingPacket(ushort header)
    {
        return !OutgoingPackets.ContainsKey(header) ? null : OutgoingPackets[header];
    }

    public PacketCache GetIncomingPacket(ushort header)
    {
        return !IncomingPackets.ContainsKey(header) ? null : IncomingPackets[header];
    }

    public void RegisterNamespace(string space, Assembly assembly = null)
    {
        _logger.LogDebug("Register packet namespace {Namespace}", space);
        if (assembly == null) assembly = Assembly.GetAssembly(typeof(DefaultPacketManager));

        var types = assembly.GetTypes().Where(t => t.Namespace?.StartsWith(space, StringComparison.Ordinal) ?? false)
            .Where(t => t.GetCustomAttribute<PacketAttribute>() != null).ToArray();
        foreach (var type in types)
        {
            Register(type);
        }
    }

    public void Register<T>()
    {
        Register(typeof(T));
    }

    private void Register(Type type)
    {
        _logger.LogDebug("Register Packet {Name}", type.Name);
        var packet = type.GetCustomAttribute<PacketAttribute>();
        if (packet == null)
        {
            SubTypes.Add(type, new SubPacketCache {
                Fields = type.GetFieldCaches(),
                HasHeader = false
            });
            return;
        }

        var cache = new PacketCache(packet.Header, type);

        var header = (ushort) packet.Header;
        if (cache.IsSubHeader)
        {
            header = (ushort) (cache.Header << 8 | cache.SubHeader);

            // We have to create packet cache for the general fields on the first packet for a header which
            // has a subheader
            if (packet.Direction.HasFlag(EDirection.Incoming))
            {
                if (!IncomingPackets.ContainsKey(header))
                {
                    IncomingPackets[packet.Header] = cache.CreateGeneralCache();
                }
            }

            if (packet.Direction.HasFlag(EDirection.Outgoing))
            {
                if (!OutgoingPackets.ContainsKey(header))
                {
                    OutgoingPackets[packet.Header] = cache.CreateGeneralCache();
                }
            }
        }

        if (packet.Direction.HasFlag(EDirection.Incoming))
        {
            if (IncomingPackets.ContainsKey(header))
            {
                _logger.LogInformation("Header 0x{PacketHeader:X2} is already in use for incoming packets. ({Name} & {TypeName})", packet.Header, type.Name, IncomingPackets[packet.Header].Type.Name);
            }
            else
            {
                IncomingPackets.Add(header, cache);
                _incomingTypes.Add(type);
            }
        }

        if (packet.Direction.HasFlag(EDirection.Outgoing))
        {
            if (OutgoingPackets.ContainsKey(header))
            {
                _logger.LogInformation("Header 0x{Header:X2} is already in use for outgoing packets. ({Name} & {TypeName})", packet.Header, type.Name, OutgoingPackets[packet.Header].Type.Name);
            }
            else
            {
                OutgoingPackets.Add(header, cache);
                _outgoingTypes.Add(type);
            }
        }

        foreach (var field in type.GetProperties().Where(x => x.PropertyType is { IsArray: true }))
        {
            Register(field.PropertyType.GetElementType());
        }
    }

    public class SubPacketCache : IPacketCache
    {
        public byte Header { get; set; }
        public bool HasHeader { get; set; }
        public FieldCache[] Fields { get; set; }
    }
}

public interface IPacketCache
{
    bool HasHeader { get; }
    byte Header { get; }
    FieldCache[] Fields { get; }
}