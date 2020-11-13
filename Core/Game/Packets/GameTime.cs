using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets
{
    [Packet(0x6a, EDirection.Outgoing)]
    public class GameTime
    {
        [Field(0)]
        public uint Time { get; set; }
    }
}