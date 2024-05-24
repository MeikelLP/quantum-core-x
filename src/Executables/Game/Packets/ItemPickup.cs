using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ClientToServerPacket(0x0F, HasSequence = true)]
public readonly ref partial struct ItemPickup
{
    public readonly uint Vid;

    public ItemPickup(uint vid)
    {
        Vid = vid;
    }
}