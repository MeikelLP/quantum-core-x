using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ClientToServerPacket(0x07, HasSequence = true)]
public readonly ref partial struct CharacterMove
{
    public readonly CharacterMovementType MovementType;
    public readonly byte Argument;
    public readonly byte Rotation;
    public readonly int PositionX;
    public readonly int PositionY;
    public readonly uint Time;

    public CharacterMove(CharacterMovementType movementType, byte argument, byte rotation, int positionX,
        int positionY, uint time)
    {
        MovementType = movementType;
        Argument = argument;
        Rotation = rotation;
        PositionX = positionX;
        PositionY = positionY;
        Time = time;
    }
}