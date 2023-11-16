using System.Reflection;
using QuantumCore.Core.Packets;

namespace QuantumCore.Game;

public class GamePacketLocationProvider : IPacketLocationProvider
{
    public IReadOnlyCollection<Assembly> GetPacketAssemblies()
    {
        return new []
        {
            typeof(GameServer).Assembly,
            typeof(GCHandshake).Assembly
        };
    }
}
