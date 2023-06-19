using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace QuantumCore.Networking;

public class NewPacketManager : INewPacketManager
{
    private readonly Dictionary<(byte Header, byte? SubHeader), PacketInfo> _infos = new();

    public NewPacketManager(IConfiguration config, Type[] packetTypes, Type[]? packetHandlerTypes = null)
    {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public;
        foreach (var packetType in packetTypes)
        {
            var header = (byte)packetType.GetProperty(nameof(IPacketSerializable.Header), flags)!.GetValue(null)!;
            var subHeader = (byte?)packetType.GetProperty(nameof(IPacketSerializable.SubHeader), flags)!.GetValue(null);

            // last or default so it can be overriden via plugins - last one is chosen
            var typeName = config.GetValue<string>("Mode") == "game"
                ? "QuantumCore.API.PluginTypes.IGamePacketHandler`1"
                : "QuantumCore.API.PluginTypes.IAuthPacketHandler`1";
            var packetHandlerType = packetHandlerTypes?
                .LastOrDefault(x =>
                    x is { IsAbstract: false, IsInterface: false } &&
                    x
                        .GetInterfaces()
                        .Any(i => i.FullName!.StartsWith(typeName) &&
                                  i.IsGenericType && i.GenericTypeArguments.First() == packetType)
                );
            _infos.Add((header, subHeader), new PacketInfo(packetType, packetHandlerType));
        }
    }

    public bool TryGetPacketInfo(in byte header, in byte? subHeader, out PacketInfo packetInfo)
    {
        return _infos.TryGetValue((header, subHeader), out packetInfo);
    }

    public bool TryGetPacketInfo(IPacketSerializable packet, out PacketInfo packetInfo)
    {
        // TODO improve - maybe with a delegate cache?
        var header = (byte)packet.GetType()
            .GetProperty(nameof(IPacketSerializable.Header), BindingFlags.Public | BindingFlags.Static)!
            .GetValue(null)!;
        var subHeader = (byte?)packet.GetType()
            .GetProperty(nameof(IPacketSerializable.SubHeader), BindingFlags.Public | BindingFlags.Static)!
            .GetValue(null);
        return _infos.TryGetValue((header, subHeader), out packetInfo);
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