using System;
using System.Linq;
using QuantumCore.API;
using QuantumCore.Core.Networking;

namespace QuantumCore.Extensions;

public static class PacketExtensions
{
    public static Type GetPacketType(this Type type)
    {
        var baseInterface = type.GetInterfaces()
            .FirstOrDefault(x => typeof(IPacketHandler).IsAssignableFrom(x) && x != typeof(IPacketHandler));

        return baseInterface?.GenericTypeArguments[0];
    }
}