using System.Reflection;
using System.Text;
using EnumsNET;
using QuantumCore.Game.Packets;
using QuantumCore.Networking;

namespace DocsGenerator;

public static class PacketDocsGenerator
{
    public static async Task GenerateAsync(string targetDir)
    {
        var packetTypes = new[] {typeof(Attack).Assembly} // ensure assembly is loaded into AppDomain
            .Concat(AppDomain.CurrentDomain.GetAssemblies())
            .Distinct()
            .SelectMany(a => a.GetTypes()
                .Where(x => typeof(IPacketSerializable).IsAssignableFrom(x) &&
                            x is {IsClass: true, IsAbstract: false} &&
                            x.GetCustomAttribute<PacketAttribute>() is not null))
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                Type = x,
                x.Name,
                Namespace = x.Namespace!,
                x.GetCustomAttribute<PacketAttribute>()!.Direction,
                x.GetCustomAttribute<PacketAttribute>()!.Header,
                x.GetCustomAttribute<SubPacketAttribute>()?.SubHeader,
                SubHeaderPosition = x.GetCustomAttribute<SubPacketAttribute>()?.Position
            })
            .ToArray();

        foreach (var packetInfo in packetTypes.AsParallel())
        {
            try
            {
                var fields = GetProperties(packetInfo.Type).ToList();

                var sb = new StringBuilder();
                sb.AppendLine($"""
                               # {packetInfo.Name}

                               |   |   |
                               |---|---|
                               |Direction|`{packetInfo.Direction.AsString()}`|
                               |Header|`0x{packetInfo.Header:X2}`|
                               """);
                if (packetInfo.SubHeader is not null)
                {
                    sb.AppendLine($"|Sub Header|`0x{packetInfo.SubHeader:X2}`|");
                }

                sb.AppendLine("""

                              ## Fields

                              """);

                if (fields.Count > 0)
                {
                    foreach (var field in fields)
                    {
                        sb.AppendLine($"* {field.Name}");
                    }
                }
                else
                {
                    sb.AppendLine("> _no fields - only headers_");
                }

                sb.AppendLine($"""

                               ```mermaid
                               ---
                               title: "{packetInfo.Name}"
                               ---
                               packet-beta
                                 0: "0x{packetInfo.Header:X2}"
                               """);
                if (packetInfo.SubHeader is not null && packetInfo.SubHeaderPosition is not null)
                {
                    fields.Insert(packetInfo.SubHeaderPosition!.Value,
                        ($"0x{packetInfo.SubHeader:X2}", typeof(byte),
                            new FieldAttribute(packetInfo.SubHeaderPosition!.Value)));
                }

                var position = 0;
                foreach (var field in fields)
                {
                    var fieldLength = GetFieldLength(field);

                    sb.AppendLine($"""
                                     {position + 1}-{position + fieldLength}: "{field.Name}"
                                   """);
                    position += fieldLength;
                }

                sb.AppendLine("```");

                var relatedPackets = packetTypes
                    .Where(x => x.Header == packetInfo.Header)
                    .ToArray();
                if (packetInfo.SubHeader is not null && relatedPackets.Length > 0)
                {
                    sb.AppendLine();
                    sb.AppendLine("## Related packets");
                    sb.AppendLine();
                    foreach (var type in relatedPackets)
                    {
                        var relativePath = GetRelativePathTo(
                            type.Namespace,
                            packetInfo.Namespace
                        );
                        if (relativePath == "") relativePath = ".";
                        sb.AppendLine($"* [{type.Name}]({relativePath}/{type.Name}.md)");
                    }
                }

                var mermaid = sb.ToString();

                await WriteFile(targetDir, packetInfo.Namespace, packetInfo.Name, mermaid);
            }
            catch (Exception)
            {
                await Console.Error.WriteLineAsync($"Failed to generate markdown docs for type {packetInfo.Name}");
                throw;
            }
        }
    }

    private static string GetRelativePathTo(string from, string to)
    {
        var fromUri = new Uri(Path.GetFullPath(from));
        var toUri = new Uri(Path.GetFullPath(to));

        var relativeUri = fromUri.MakeRelativeUri(toUri);
        var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

        return relativePath.Replace('/', Path.DirectorySeparatorChar);
    }

    private static (string Name, Type PropertyType, FieldAttribute Attribute)[] GetProperties(Type type)
    {
        return type.GetProperties()
            .Where(x => x.GetCustomAttribute<FieldAttribute>() is not null)
            .Select(x => (x.Name, x.PropertyType, Attribute: x.GetCustomAttribute<FieldAttribute>()!))
            .OrderBy(x => x.Attribute.Position)
            .ToArray();
    }


    private static int GetFieldLength((string Name, Type PropertyType, FieldAttribute Attribute) field)
    {
        // if explicit length
        if (field.Attribute.Length is not -1)
        {
            return field.Attribute.Length;
        }

        // if array
        var propertyType = field.PropertyType;
        if (field.PropertyType.IsArray && field.Attribute.ArrayLength is not -1)
        {
            var elementType = field.PropertyType.GetElementType()!;
            if (elementType.IsPrimitive)
            {
                return GetPrimitiveSize(elementType) * field.Attribute.ArrayLength;
            }

            var props = GetProperties(elementType);
            return props.Sum(GetFieldLength) * field.Attribute.ArrayLength;
        }

        if (propertyType.IsEnum)
        {
            propertyType = propertyType.GetEnumUnderlyingType();
        }

        if (propertyType.IsPrimitive)
        {
            return GetPrimitiveSize(propertyType);
        }

        // try custom type
        var subProps = GetProperties(propertyType);
        var multiplier = propertyType.IsArray ? field.Attribute.ArrayLength : 1;
        return subProps.Sum(GetFieldLength) * multiplier;
    }

    private static int GetPrimitiveSize(Type type)
    {
        if (type == typeof(long) ||
            type == typeof(ulong) ||
            type == typeof(double))
        {
            return 4;
        }

        if (type == typeof(int) ||
            type == typeof(uint) ||
            type == typeof(float))
        {
            return 4;
        }

        if (type == typeof(short) ||
            type == typeof(ushort))
        {
            return 2;
        }

        if (type == typeof(byte) ||
            type == typeof(sbyte) ||
            type == typeof(bool))
        {
            return 1;
        }

        throw new InvalidOperationException($"{type.FullName} is not a primitive type");
    }

    private static async Task WriteFile(string baseDir, string directory, string name, string content)
    {
        var packetsDir = Path.Combine(baseDir, "Packets");
        var relativeDir = Path.Combine(packetsDir, directory);
        if (!Directory.Exists(relativeDir))
        {
            Directory.CreateDirectory(relativeDir);
        }

        var relativePath = Path.Combine(relativeDir, $"{name}.md");
        var filePath = Path.GetFullPath(Path.Combine(relativeDir, relativePath));
        await File.WriteAllTextAsync(filePath, content);
        Console.WriteLine(filePath);
    }
}
