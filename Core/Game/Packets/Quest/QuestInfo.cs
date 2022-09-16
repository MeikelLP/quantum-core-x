using System;
using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets.Quest;

[Packet(0x51, EDirection.Outgoing)]
public class QuestInfo
{
    [Flags]
    public enum InfoFlags
    {
        Begin = 1 << 0,
        Title = 1 << 1,
        CounterName = 1 << 4,
        CounterValue = 1 << 5
    }
    
    [Field(0)]
    [Size]
    public ushort Size { get; set; }
    
    [Field(1)]
    public ushort Index { get; set; }
    
    [Field(2)]
    public byte Flags { get; set; }
    
    [Dynamic]
    public byte[] Data { get; set; }
}