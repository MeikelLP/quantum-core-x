using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.Core.Packets;

namespace QuantumCore.Extensions;

public static class ConnectionExtensions
{
    public static void SetPhase(this IConnection connection, EPhase phase)
    {
        connection.Phase = phase;
        connection.Send(new GCPhase
        {
            Phase = (byte)connection.Phase
        });
    }
}
