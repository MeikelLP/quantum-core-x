using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x04, EDirection.Incoming, Sequence = true)]
    [PacketGenerator]
    public partial class CreateCharacter
    {
        [Field(0)] public byte Slot { get; set; }

        [Field(1, Length = PlayerConstants.PLAYER_NAME_MAX_LENGTH)]
        public string Name { get; set; } = "";

        [Field(2)] public ushort Class { get; set; }
        [Field(3)] public byte Appearance { get; set; }
        [Field(4, ArrayLength = 4)] public byte[] Unknown { get; set; } = new byte[4];
    }
}