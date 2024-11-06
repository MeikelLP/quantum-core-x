using System.ComponentModel.DataAnnotations;

namespace QuantumCore;

public class HostingOptions
{
    public const string ModeAuth = "auth";
    public const string ModeGame = "game";
    [Required] public ushort Port { get; set; }
    public string? IpAddress { get; set; }
}