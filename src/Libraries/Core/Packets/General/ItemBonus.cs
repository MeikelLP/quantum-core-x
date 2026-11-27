using QuantumCore.Networking;

namespace QuantumCore.Packets.General;

public class ItemBonus
{
    [Field(0)] public byte BonusId { get; set; }
    [Field(1)] public ushort Value { get; set; }
}
