using System.Threading.Tasks;
using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.Core.Packets;

namespace QuantumCore.Extensions;

public static class ConnectionExtensions
{
    public static async Task SetPhaseAsync(this IConnection connection, EPhases phase)
    {
        connection.Phase = phase;
        await connection.Send(new GCPhase
        {
            Phase = connection.Phase
        });
    }
}