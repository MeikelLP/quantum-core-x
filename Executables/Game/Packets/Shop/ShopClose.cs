using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets.Shop
{
    [Packet(0x32, EDirection.Incoming, Sequence = true)]
    [SubPacket(0x00, 0)]
    public class ShopClose
    {
        
    }
}