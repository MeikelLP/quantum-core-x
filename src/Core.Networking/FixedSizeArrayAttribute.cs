namespace QuantumCore.Networking;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FixedSizeArrayAttribute : Attribute
{
    public int Size { get; }

    public FixedSizeArrayAttribute(int size)
    {
        Size = size;
    }
}