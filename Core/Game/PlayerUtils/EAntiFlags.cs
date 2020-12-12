using System;

namespace QuantumCore.Game.PlayerUtils
{
    [Flags]
    public enum EAntiFlags
    {
        Female = (1 << 0),
        Male = (1 << 1),
        Warrior = (1 << 2),
        Assassin = (1 << 3),
        Sura = (1 << 4),
        Shaman = (1 << 5),
        Drop = (1 << 7),
        Sell = (1 << 8),
        Safebox = (1 << 17),
    }
}