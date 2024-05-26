using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ClientToServerPacket(0x6d, HasSequence = true)]
public partial class TokenLogin
{
    [FixedSizeString(31)] public required string Username { get; set; }
    public uint Key { get; set; }
    public uint[] Xteakeys { get; set; } = [];

    public override string ToString()
    {
        return $" Username = {Username}, Key = {Key}";
    }
}