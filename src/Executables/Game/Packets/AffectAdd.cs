using QuantumCore.API.Game.Types;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x7E, EDirection.Outgoing)]
[PacketGenerator]
public partial class AffectAdd
{
    public uint Type { get; set; }
    public EAffectType ApplyOn { get; set; }
    public uint ApplyValue { get; set; }
    public uint Flag { get; set; }
    public uint Duration { get; set; }
    public uint SpCost { get; set; }
}
