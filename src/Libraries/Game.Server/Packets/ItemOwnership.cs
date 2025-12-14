using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x1F, EDirection.Outgoing)]
[PacketGenerator]
public partial class ItemOwnership
{
    public uint Vid { get; set; }

    [Field(1, Length = PlayerConstants.PlayerNameMaxLength)] public string Player { get; set; }
}
