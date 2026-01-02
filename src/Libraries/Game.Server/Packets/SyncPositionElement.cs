using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

public struct SyncPositionElement
{
    [Field(0)] public uint Vid { get; set; }
    [Field(1)] public int X { get; set; }
    [Field(2)] public int Y { get; set; }
}
