using System;
using System.Threading;
using System.Threading.Tasks;
using QuantumCore.API.Game.Types;
using QuantumCore.Networking;

namespace QuantumCore.API
{
    public interface IConnection
    {
        Guid Id { get; }
        EPhases Phase { get; set; }
        Task ExecuteTask { get; }
        void Close(bool expected = true);
        void Send<T>(T packet) where T : IPacketSerializable;
        Task StartAsync(CancellationToken token = default);
    }
}
