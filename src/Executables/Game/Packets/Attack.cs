using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ClientToServerPacket(0x02, HasSequence = true)]
public readonly ref partial struct Attack
{
    public readonly byte AttackType;
    public readonly uint Vid;
    public readonly ushort Unknown;

    public Attack(byte attackType, uint vid, ushort unknown)
    {
        AttackType = attackType;
        Vid = vid;
        Unknown = unknown;
    }
}