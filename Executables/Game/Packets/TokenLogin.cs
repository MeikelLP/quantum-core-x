using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets
{
    [Packet(0x6d, EDirection.Incoming, Sequence = true)]
    public class TokenLogin
    {
        [Field(0, Length = 31)]
        public string Username { get; set; }
        [Field(1)]
        public uint Key { get; set; }
        [Field(2, ArrayLength = 4)]
        public uint[] Xteakeys { get; set; }

        public override string ToString()
        {
            return base.ToString() + $" Username = {Username}, Key = {Key}";
        }
    }
}