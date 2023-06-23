using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.General;

public class QuickSlot
{
    [Field(0)]
    public byte Type { get; set; }
    
    /// <summary>
    /// Position of item in inventory or skill depending on the type
    /// </summary>
    [Field(1)]
    public byte Position { get; set; }
}