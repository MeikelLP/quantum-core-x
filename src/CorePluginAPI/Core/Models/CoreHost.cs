using System.Net;

namespace QuantumCore.API.Core.Models;

public record struct CoreHost(IPAddress Ip, ushort Port);