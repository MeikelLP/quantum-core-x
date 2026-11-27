using System.ComponentModel.DataAnnotations;

namespace QuantumCore;

public class HostingOptions
{
    public const string MODE_AUTH = "auth";
    public const string MODE_GAME = "game";
    [Required] public ushort Port { get; set; }
    public string? IpAddress { get; set; }
}
