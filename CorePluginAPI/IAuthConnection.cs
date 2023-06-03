using System.Threading.Tasks;
using QuantumCore.API.Core.Models;

namespace QuantumCore.API;

public interface IAuthConnection : IConnection
{
    Task<bool> HandleHandshake(GCHandshakeData handshake);
}