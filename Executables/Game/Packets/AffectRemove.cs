using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets.Affects
{
    [Packet(0x7F, EDirection.Outgoing)]
    public class AffectRemove
    {
        [Field(0)]
        public uint Type { get; set; }
        [Field(1)]
        public byte ApplyOn { get; set; }
    }
}
