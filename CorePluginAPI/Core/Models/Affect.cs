using System;

namespace QuantumCore.API.Core.Models;

public class Affect
{
    public Guid PlayerId { get; set; }
    public long Type { get; set; }
    public byte ApplyOn { get; set; }
    public int ApplyValue { get; set; }
    public int Flag { get; set; }
    public DateTime Duration { get; set; }
    public int SpCost { get; set; }
}