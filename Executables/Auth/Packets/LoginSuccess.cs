﻿using QuantumCore.Core.Packets;

namespace QuantumCore.Auth.Packets
{
    [Packet(0x96, EDirection.Outgoing)]
    public class LoginSuccess
    {
        [Field(0)]
        public uint Key { get; set; }
        [Field(1)]
        public byte Result { get; set; }
    }
}