﻿using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x71, EDirection.Outgoing)]
    [PacketGenerator]
    public partial class CharacterDetails
    {
        [Field(0)] public uint Vid { get; set; }
        [Field(1)] public ushort Class { get; set; }

        [Field(2, Length = PlayerConstants.PLAYER_NAME_MAX_LENGTH)]
        public string Name { get; set; } = "";

        [Field(3)] public int PositionX { get; set; }
        [Field(4)] public int PositionY { get; set; }
        [Field(5)] public int PositionZ { get; set; }
        [Field(6)] public byte Empire { get; set; }
        [Field(7)] public byte SkillGroup { get; set; }
    }
}