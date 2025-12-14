using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x47, EDirection.OUTGOING)]
[PacketGenerator]
public partial class ProjectilePacket
{
    public uint Shooter { get; set; }
    public uint Target { get; set; }
    public int TargetX { get; set; }
    public int TargetY { get; set; }
}
