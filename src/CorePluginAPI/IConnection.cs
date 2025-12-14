using QuantumCore.API.Game.Types;
using QuantumCore.Networking;

namespace QuantumCore.API;

public interface IConnection
{
    Guid Id { get; }
    EPhase Phase { get; set; }
    Task ExecuteTask { get; }
    void Close(bool expected = true);
    void Send<T>(T packet) where T : IPacketSerializable;
    Task StartAsync(CancellationToken token = default);
}