using System;
using QuantumCore.Core.Packets;

namespace QuantumCore.Auth.Packets {
    [Packet(0x6f, EDirection.Incoming)]
    class LoginRequest {
        [Field(0, 31)]
        public string Username { get; set; }
        [Field(1, 17)]
        public string Password { get; set; }
        [Field(2, 4)]
        public UInt32[] EncryptKey { get; set; }
    }
}