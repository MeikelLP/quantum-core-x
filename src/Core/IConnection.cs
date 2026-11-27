using QuantumCore.Game.Types;
using QuantumCore.Networking;

namespace QuantumCore;

public interface IConnection
{
    Guid Id { get; }
    EPhase Phase { get; set; }
    Task? ExecuteTask { get; }
    void Close(bool expected = true);
    void Send<T>(T packet) where T : IPacketSerializable;
    Task StartAsync(CancellationToken token = default);
}
