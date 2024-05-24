namespace QuantumCore.Game.Packets.General;

public readonly struct QuickSlot
{
    public readonly byte Type;

    /// <summary>
    /// Position of item in inventory or skill depending on the type
    /// </summary>
    public readonly byte Position;

    public QuickSlot(byte type, byte position)
    {
        Type = type;
        Position = position;
    }
}