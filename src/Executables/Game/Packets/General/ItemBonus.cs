namespace QuantumCore.Game.Packets.General;

public readonly struct ItemBonus
{
    public readonly byte BonusId;
    public readonly ushort Value;

    public ItemBonus(byte bonusId, ushort value)
    {
        BonusId = bonusId;
        Value = value;
    }
}