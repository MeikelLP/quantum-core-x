using System;
using System.Threading;
using System.Threading.Tasks;
using QuantumCore.API.Game.Types;

namespace QuantumCore.API
{
    public interface IConnection
    {
        Guid Id { get; }
        EPhases Phase { get; set; }
        Task ExecuteTask { get; }
        void Close();
        Task Send(object packet);
        Task StartAsync(CancellationToken token = default);
    }
}