using System.Reflection;
using QuantumCore.Core.Packets;

namespace QuantumCore.Auth;

public class AuthPacketLocationProvider : IPacketLocationProvider
{
    public IReadOnlyCollection<Assembly> GetPacketAssemblies()
    {
        return
        [
            typeof(AuthServer).Assembly,
            typeof(GcHandshake).Assembly
        ];
    }
}