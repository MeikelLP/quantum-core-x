using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x87)]
public readonly ref partial struct DamageInfo
{
    public readonly uint Vid;
    public readonly byte DamageFlags;
    public readonly int Damage;

    public DamageInfo(uint vid, byte damageFlags, int damage)
    {
        Vid = vid;
        DamageFlags = damageFlags;
        Damage = damage;
    }
}