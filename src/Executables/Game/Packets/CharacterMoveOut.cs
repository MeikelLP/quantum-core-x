using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x03, HasSequence = true)]
public readonly ref partial struct CharacterMoveOut
{
    public readonly CharacterMovementType MovementType;
    public readonly byte Argument;
    public readonly byte Rotation;
    public readonly uint Vid;
    public readonly int PositionX;
    public readonly int PositionY;
    public readonly uint Time;
    public readonly uint Duration;

    public CharacterMoveOut(CharacterMovementType movementType, byte argument, byte rotation, uint vid,
        int positionX, int positionY, uint time, uint duration)
    {
        MovementType = movementType;
        Argument = argument;
        Rotation = rotation;
        Vid = vid;
        PositionX = positionX;
        PositionY = positionY;
        Time = time;
        Duration = duration;
    }
}