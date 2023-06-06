using JetBrains.Annotations;

namespace QuantumCore.Networking;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
public class PacketGeneratorAttribute : Attribute
{
}