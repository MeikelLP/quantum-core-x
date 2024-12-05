namespace QuantumCore.Networking;

[AttributeUsage(AttributeTargets.Field)]
public class DynamicSizeFieldAttribute : Attribute;

[AttributeUsage(AttributeTargets.Field)]
public class FixedSizeArrayAttribute : Attribute
{
    public int Length { get; }

    public FixedSizeArrayAttribute(int length)
    {
        Length = length;
    }
}
[AttributeUsage(AttributeTargets.Field)]
public class FixedSizeStringAttribute : Attribute
{
    public int Length { get; }

    public FixedSizeStringAttribute(int length)
    {
        Length = length;
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class FieldOrderAttribute : Attribute
{
    public int Position { get; }

    public FieldOrderAttribute(int position)
    {
        Position = position;
    }
}
