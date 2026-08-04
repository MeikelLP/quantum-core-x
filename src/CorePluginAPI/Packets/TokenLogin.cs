using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x6d, EDirection.INCOMING, Sequence = true)]
[PacketGenerator]
public partial class TokenLogin
{
    [Field(0, Length = 31)] public string Username { get; set; } = "";
    [Field(1)] public uint Key { get; set; }

    [Field(2)] public uint[] Xteakeys { get; set; } = new uint[4];

    public override string ToString()
    {
        return base.ToString() + $" Username = {Username}, Key = {Key}";
    }
}
