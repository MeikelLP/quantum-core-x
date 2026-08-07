using QuantumCore.API;

namespace QuantumCore.Game.Extensions;

public static class ConnectionExtensions
{
    public static void ForAllConnections(this IGameServer gameServer, Action<IConnection> callback)
    {
        ArgumentNullException.ThrowIfNull(gameServer);
        ArgumentNullException.ThrowIfNull(callback);
        foreach (var connection in gameServer.Connections)
        {
            callback(connection);
        }
    }
}