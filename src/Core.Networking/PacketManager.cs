using System.Reflection;
using Microsoft.Extensions.Logging;

namespace QuantumCore.Networking;

public class PacketManager : IPacketManager
{
    private readonly Dictionary<(byte Header, byte? SubHeader), PacketInfo> _infos = new();
    private readonly Dictionary<Type, (byte Header, byte? SubHeader)> _typeCache = new();

    public PacketManager(ILogger<PacketManager> logger, IEnumerable<Type> packetTypes, Type[]? packetHandlerTypes = null)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
        foreach (var packetType in packetTypes)
        {
            var packetInstance = Activator.CreateInstance(packetType);
            var header = (byte)packetType.GetProperty(nameof(IPacketSerializable.Header), flags)!.GetValue(packetInstance)!;
            var subHeader = (byte?)packetType.GetProperty(nameof(IPacketSerializable.SubHeader), flags)!.GetValue(packetInstance);
            var hasStaticSize = (bool)packetType.GetProperty(nameof(IPacketSerializable.HasStaticSize), flags)!.GetValue(packetInstance)!;
            var hasSequence = (bool)packetType.GetProperty(nameof(IPacketSerializable.HasSequence), flags)!.GetValue(packetInstance)!;

            // last or default, so it can be overriden via plugins - last one is chosen
            var packetHandlerType = packetHandlerTypes?
                .LastOrDefault(x =>
                    x is { IsAbstract: false, IsInterface: false } &&
                    x
                        .GetInterfaces()
                        .Any(i => i.IsGenericType && i.GenericTypeArguments.First() == packetType)
                );
            _infos.Add((header, subHeader), new PacketInfo(packetType, packetHandlerType, hasStaticSize, hasSequence));
            if (subHeader.HasValue)
            {
                logger.LogDebug("Registered header 0x{Header:X2}|0x{SubHeader:X2} with handler {HandlerType}", header,
                    subHeader, packetHandlerType);
            }
            else
            {
                logger.LogDebug("Registered header 0x{Header:X2} with handler {HandlerType}", header,
                    packetHandlerType);
            }
        }
    }

    public bool TryGetPacketInfo(in byte header, in byte? subHeader, out PacketInfo packetInfo)
    {
        return _infos.TryGetValue((header, subHeader), out packetInfo);
    }

    public bool TryGetPacketInfo(IPacketSerializable packet, out PacketInfo packetInfo)
    {
        if(!_typeCache.TryGetValue(packet.GetType(), out var pair))
        {
            pair.Header = packet.Header;
            pair.SubHeader = packet.SubHeader;
            _typeCache.Add(packet.GetType(), (pair.Header, pair.SubHeader));
        }
        return _infos.TryGetValue((pair.Header, pair.SubHeader), out packetInfo);
    }

    public bool IsSubPacketDefinition(in byte header)
    {
        foreach (var key in _infos.Keys)
        {
            if (key.Header == header && key.SubHeader.HasValue)
            {
                return true;
            }
        }

        return false;
    }
}
