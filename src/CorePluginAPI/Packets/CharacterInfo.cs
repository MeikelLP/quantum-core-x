using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.Types.Entities;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x88, EDirection.OUTGOING)]
[PacketGenerator]
public partial class CharacterInfo
{
    [Field(0)] public uint Vid { get; set; }

    [Field(1, Length = PlayerConstants.PLAYER_NAME_MAX_LENGTH)]
    public string Name { get; set; } = "";

    [Field(2, ArrayLength = 4)] public ushort[] Parts { get; set; } = new ushort[4];
    [Field(3)] public EEmpire Empire { get; set; }
    [Field(4)] public uint GuildId { get; set; }
    [Field(5)] public uint Level { get; set; }
    [Field(6)] public short RankPoints { get; set; }
    [Field(7)] public EPvpMode PvpMode { get; set; }
    [Field(8)] public uint MountVnum { get; set; }
}
