using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets.Affects
{
    [Packet(0x7E, EDirection.Outgoing)]
    public class AffectAdd
    {
<<<<<<< HEAD
        [Field(0, ArrayLength = 1)]
        public AffectAddPacket[] Elem { get; set; } = new AffectAddPacket[1];
=======
        [Field(0)]
        public AffectAddPacket Elem { get; set; }
>>>>>>> a2b5a04 (affect table->db, affect packets add/remove)
    }
    public class AffectAddPacket
    {
        [Field(0)]
        public uint Type { get; set; }
        [Field(1)]
        public byte ApplyOn { get; set; }
        [Field(2)]
        public uint ApplyValue { get; set; }
        [Field(3)]
        public uint Flag { get; set; }
        [Field(4)]
        public uint Duration { get; set; }
<<<<<<< HEAD
        [Field(5)]
        public uint SpCost { get; set; } = 0;
=======
>>>>>>> a2b5a04 (affect table->db, affect packets add/remove)
    }
}
