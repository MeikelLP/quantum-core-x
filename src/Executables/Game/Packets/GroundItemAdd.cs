using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x1A)]
public readonly ref partial struct GroundItemAdd
{
    public readonly int PositionX;
    public readonly int PositionY;
    public readonly int PositionZ;
    public readonly uint Vid;
    public readonly uint ItemId;

    public GroundItemAdd(int positionX, int positionY, int positionZ, uint vid, uint itemId)
    {
        PositionX = positionX;
        PositionY = positionY;
        PositionZ = positionZ;
        Vid = vid;
        ItemId = itemId;
    }
}