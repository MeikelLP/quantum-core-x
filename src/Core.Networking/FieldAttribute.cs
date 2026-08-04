namespace QuantumCore.Networking;

[AttributeUsage(AttributeTargets.Property)]
public sealed class FieldAttribute : Attribute
{
    public FieldAttribute(int position)
    {
        Position = position;
    }

    public int Position { get; }
    public int Length { get; set; } = -1;
    public int ArrayLength { get; set; } = -1;
}