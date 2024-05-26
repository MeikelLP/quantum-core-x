using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;

namespace QuantumCore.API
{
    public interface IConnection
    {
        Guid Id { get; }
        EPhases Phase { get; set; }
        Task ExecuteTask { get; }
        void Close(bool expected = true);
        void Send(byte[] packet);
        Task StartAsync(CancellationToken token = default);
        bool HandleHandshake(GCHandshakeData handshake);
    }
}
