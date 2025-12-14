using QuantumCore.API.Game.Guild;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Guild;

[Packet(0x4B, EDirection.Outgoing)]
[SubPacket(0x09, 1)]
[PacketGenerator]
public partial class GuildNewsPacket
{
    [Field(0)]
    public ushort Size =>
        (ushort) ((sizeof(uint) + PlayerConstants.PlayerNameMaxLength + 1 + GuildConstants.NewsCountMax + 1) *
                  News.Length);

    [Field(1)] public byte Count => (byte) News.Length;
    [Field(2)] public GuildNews[] News { get; set; } = [];
}

public class GuildNews
{
    [Field(0)] public uint NewsId { get; set; }

    [Field(1, Length = PlayerConstants.PlayerNameMaxLength)]
    public string PlayerName { get; set; } = "";

    [Field(2, Length = GuildConstants.NewsMessageMaxLength + 1)]
    public string Message { get; set; } = "";
}