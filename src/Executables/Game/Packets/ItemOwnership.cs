using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x1F)]
public readonly ref partial struct ItemOwnership
{
    public readonly uint Vid;
    [FixedSizeString(25)] public readonly string Player;

    public ItemOwnership(uint vid, string player)
    {
        Vid = vid;
        Player = player;
    }
}