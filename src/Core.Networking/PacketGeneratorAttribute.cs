namespace QuantumCore.Networking;

/// <summary>
/// Adding this attribute to a type definition will cause it to be extended by and Serialize &amp; GetSize method
/// The type must be declared as partial and must be a top level class/struct/... - no sub-classes
/// </summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public class PacketGeneratorAttribute : Attribute
{
}
