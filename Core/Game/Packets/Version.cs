﻿using QuantumCore.Core.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0xf1, EDirection.Incoming, Sequence = true)]
    public class Version
    {
        [Field(0, Length = 33)]
        public string ExecutableName { get; set; }
        [Field(1, Length = 33)]
        public string Timestamp { get; set; }
    }
}