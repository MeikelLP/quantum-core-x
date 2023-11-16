using System.Reflection;
using QuantumCore.Core.Packets;

namespace QuantumCore.Auth;

public class AuthPacketLocationProvider : IPacketLocationProvider
{
    public IReadOnlyCollection<Assembly> GetPacketAssemblies()
    {
        return new[]
        {
            typeof(AuthServer).Assembly,
            typeof(GCHandshake).Assembly
        };
    }
}
