using QuantumCore.API.Game.Types;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x7F, EDirection.Outgoing)]
[PacketGenerator]
public partial class AffectRemove
{
    public uint Type { get; set; }
    public EApplyType ApplyOn { get; set; }
}
