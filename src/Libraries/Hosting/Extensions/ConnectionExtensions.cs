using QuantumCore.Core.Packets;
using QuantumCore.Game.Types;

namespace QuantumCore.Extensions;

public static class ConnectionExtensions
{
    public static void SetPhase(this IConnection connection, EPhase phase)
    {
        ArgumentNullException.ThrowIfNull(connection);
        connection.Phase = phase;
        connection.Send(new GcPhase { Phase = connection.Phase });
    }
}