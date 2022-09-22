using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QuantumCore.Core.Packets;
using QuantumCore.Core.Utils;
using Serilog;

namespace QuantumCore.Core.Networking;

public class DefaultPacketManager : IPacketManager
{
    private readonly List<Type> _incomingTypes = new();
    private readonly List<Type> _outgoingTypes = new();
    public Dictionary<ushort, PacketCache> OutgoingPackets {get;} = new();
    public Dictionary<ushort, PacketCache> IncomingPackets {get;} = new();

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
        Log.Debug($"Register packet namespace {space}");
        if (assembly == null) assembly = Assembly.GetAssembly(typeof(DefaultPacketManager));

        var types = assembly.GetTypes().Where(t => t.Namespace?.StartsWith(space, StringComparison.Ordinal) ?? false)
            .Where(t => t.GetCustomAttribute<PacketAttribute>() != null).ToArray();
        foreach (var type in types)
        {
            Log.Debug($"Register Packet {type.Name}");
            var packet = type.GetCustomAttribute<PacketAttribute>();
            if (packet == null)
            {
                continue;
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
                    Log.Information(
                        $"Header 0x{packet.Header} is already in use for incoming packets. ({type.Name} & {IncomingPackets[packet.Header].Type.Name})");
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
                    Log.Information(
                        $"Header 0x{packet.Header} is already in use for outgoing packets. ({type.Name} & {OutgoingPackets[packet.Header].Type.Name})");
                }
                else
                {
                    OutgoingPackets.Add(header, cache);
                    _outgoingTypes.Add(type);
                }
            }
        }
    }
}