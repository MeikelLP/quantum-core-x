using System.ComponentModel.DataAnnotations;

namespace QuantumCore;

public class HostingOptions
{
    [Required]
    public int Port { get; set; }
    public string? IpAddress { get; set; }
}
