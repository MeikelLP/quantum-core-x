using QuantumCore.API.Game.Types;
using QuantumCore.Networking;

namespace QuantumCore.API
{
    public interface IConnection
    {
        Guid Id { get; }
        EPhases Phase { get; set; }
        Task ExecuteTask { get; }
        void Close();
        void Send<T>(T packet) where T : IPacketSerializable;
        Task StartAsync(CancellationToken token = default);
    }
}
