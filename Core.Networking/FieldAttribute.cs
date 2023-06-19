namespace QuantumCore.Core.Networking;

[AttributeUsage(AttributeTargets.Property)]
public class FieldAttribute : Attribute
{
    public FieldAttribute(int position)
    {
        Position = position;
    }

    public int Position { get; set; }
    public int Length { get; set; } = -1;
    public int ArrayLength { get; set; } = -1;
    public Type EnumType { get; set; }
}