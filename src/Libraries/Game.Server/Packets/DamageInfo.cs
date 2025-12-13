using QuantumCore.API.Game.Types.Combat;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x87, EDirection.Outgoing)]
    [PacketGenerator]
    public partial class DamageInfo
    {
        [Field(0)] public uint Vid { get; set; }
        [Field(1)] public EDamageFlags DamageFlags { get; set; }
        [Field(2)] public int Damage { get; set; }
    }
}
