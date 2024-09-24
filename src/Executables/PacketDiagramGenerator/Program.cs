using System.Reflection;
using System.Text;
using QuantumCore.Game.Packets;
using QuantumCore.Networking;

var packetTypes = new[] {typeof(Attack).Assembly}.Concat(AppDomain.CurrentDomain.GetAssemblies())
    .Distinct()
    .SelectMany(a => a.GetTypes()
        .Where(x => typeof(IPacketSerializable).IsAssignableFrom(x) &&
                    x is {IsClass: true, IsAbstract: false} &&
                    x.GetCustomAttribute<PacketAttribute>() is not null))
    .OrderBy(x => x.Name)
    .ToArray();

foreach (var packetType in packetTypes)
{
    var fields = GetProperties(packetType).ToList();
    var meta = packetType.GetCustomAttribute<PacketAttribute>()!;
    var sub = packetType.GetCustomAttribute<SubPacketAttribute>();

    var sb = new StringBuilder();
    sb.AppendLine($"""
                   # {packetType.Name}

                   ```mermaid
                   ---
                   title: "{packetType.Name}"
                   ---
                   packet-beta
                     0: "0x{meta.Header:X2}"
                   """);
    if (sub is not null)
    {
        fields.Insert(sub.Position, ($"0x{sub.SubHeader:X2}", typeof(byte), new FieldAttribute(sub.Position)));
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

    var mermaid = sb.ToString();

    await WriteFile(packetType.Namespace!, packetType.Name, mermaid);
}

return;

(string Name, Type PropertyType, FieldAttribute Attribute)[] GetProperties(Type type)
{
    return type.GetProperties()
        .Where(x => x.GetCustomAttribute<FieldAttribute>() is not null)
        .Select(x => (x.Name, x.PropertyType, Attribute: x.GetCustomAttribute<FieldAttribute>()!))
        .OrderBy(x => x.Attribute.Position)
        .ToArray();
}


int GetFieldLength((string Name, Type PropertyType, FieldAttribute Attribute) field)
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
    return subProps.Sum(GetFieldLength) * field.Attribute.ArrayLength;
}

int GetPrimitiveSize(Type type)
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

async Task WriteFile(string directory, string name, string content)
{
    var currentDirectory = Directory.GetCurrentDirectory();
    if (currentDirectory.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar))
    {
        // get out of the bin directory in development
        currentDirectory = Path.GetFullPath(Path.Combine(currentDirectory, "..", "..", ".."));
    }

    var baseDir = Path.Combine(currentDirectory, "..", "..", "..", "docs", "docs", "Packets");
    var relativeDir = Path.Combine(baseDir, directory);
    if (!Directory.Exists(relativeDir))
    {
        Directory.CreateDirectory(relativeDir);
    }

    var relativePath = Path.Combine(relativeDir, $"{name}.md");
    var filePath = Path.GetFullPath(Path.Combine(relativeDir, relativePath));
    await File.WriteAllTextAsync(filePath, content);
    Console.WriteLine(filePath);
}
