using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x0B, EDirection.Outgoing)]
    [PacketGenerator]
    public partial class DeleteCharacterFail
    {
    }
}
