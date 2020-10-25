using System;

namespace QuantumCore.Core.Packets {
    [Packet(0xff, EDirection.Incoming | EDirection.Outgoing)]
    public class GCHandshake {
        [Field(0)]
        public UInt32 Handshake { get; set; }
        [Field(1)]
        public UInt32 Time { get; set; }
        [Field(2)]
        public UInt32 Delta { get; set; }
    }
}