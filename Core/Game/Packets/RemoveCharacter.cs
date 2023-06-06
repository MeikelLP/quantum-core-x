using QuantumCore.Core.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x02, EDirection.Outgoing)]
    public class RemoveCharacter
    {
        [Field(0)]
        public uint Vid { get; set; }
    }
}