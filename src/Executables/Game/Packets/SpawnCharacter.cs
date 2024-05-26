using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x01)]
public readonly ref partial struct SpawnCharacter
{
    public readonly uint Vid;
    public readonly float Angle;
    public readonly int PositionX;
    public readonly int PositionY;
    public readonly int PositionZ;
    public readonly byte CharacterType;
    public readonly ushort Class;
    public readonly byte MoveSpeed;
    public readonly byte AttackSpeed;
    public readonly CharacterMovementType State;
    [FixedSizeArray(2)] public readonly uint[] Affects;

    public SpawnCharacter(uint vid, float angle, int positionX, int positionY, int positionZ, byte characterType,
        ushort @class, byte moveSpeed, byte attackSpeed, CharacterMovementType state, uint[] affects)
    {
        Vid = vid;
        Angle = angle;
        PositionX = positionX;
        PositionY = positionY;
        PositionZ = positionZ;
        CharacterType = characterType;
        Class = @class;
        MoveSpeed = moveSpeed;
        AttackSpeed = attackSpeed;
        State = state;
        Affects = affects;
    }
}