using System.Reflection;
using QuantumCore.API.Packets;
using QuantumCore.Core.Packets;

namespace QuantumCore.Game;

public class GamePacketLocationProvider : IPacketLocationProvider
{
    public IReadOnlyCollection<Assembly> GetPacketAssemblies()
    {
        return [typeof(Attack).Assembly, typeof(GameServer).Assembly, typeof(GcHandshake).Assembly];
    }
}