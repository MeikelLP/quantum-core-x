namespace QuantumCore.Networking;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FixedSizeStringAttribute : Attribute
{
    public int Size { get; }

    public FixedSizeStringAttribute(int size)
    {
        Size = size;
    }
}