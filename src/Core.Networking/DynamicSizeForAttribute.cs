namespace QuantumCore.Networking;

[AttributeUsage(AttributeTargets.Field)]
public class DynamicSizeForAttribute : Attribute
{
    public string TargetFieldName { get; }

    public DynamicSizeForAttribute(string targetFieldName)
    {
        TargetFieldName = targetFieldName;
    }
}