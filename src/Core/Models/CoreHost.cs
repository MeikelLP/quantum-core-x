using System.Net;

namespace QuantumCore.Models;

public record struct CoreHost(IPAddress Ip, ushort Port);