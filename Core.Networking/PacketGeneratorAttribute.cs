using JetBrains.Annotations;

namespace QuantumCore.Networking;

/// <summary>
/// Adding this attribute to a type definition will cause it to be extended by and Serialize & GetSize method
/// The type must be declared as partial and must be a top level class/struct/... - no sub-classes
/// </summary>
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public class PacketGeneratorAttribute : Attribute
{
}