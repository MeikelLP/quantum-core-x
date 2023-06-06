using System;
using System.Linq;
using System.Reflection;
using QuantumCore.API.PluginTypes;
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

    public static FieldCache[] GetFieldCaches(this Type type)
    {
        return type
            .GetProperties()
            .Where(field => field.GetCustomAttribute<FieldAttribute>() is not null)
            .OrderBy(field => field.GetCustomAttribute<FieldAttribute>()!.Position)
            .Select(field => new FieldCache(field))
            .ToArray();
    }
}