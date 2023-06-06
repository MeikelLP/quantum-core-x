using QuantumCore.Core.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x07, EDirection.Incoming, Sequence = true)]
    public class CharacterMove
    {
        public enum CharacterMovementType
        {
            Wait = 0,
            Move = 1,
            Attack = 2,
            Combo = 3,
            MobSkill = 4,
            Max,
            Skill = 0x80
        }
        
        [Field(0)]
        public byte MovementType { get; set; }
        [Field(1)]
        public byte Argument { get; set; }
        [Field(2)]
        public byte Rotation { get; set; }
        [Field(3)]
        public int PositionX { get; set; }
        [Field(4)]
        public int PositionY { get; set; }
        [Field(5)]
        public uint Time { get; set; }
    }
}