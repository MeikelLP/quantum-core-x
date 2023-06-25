using JetBrains.Annotations;

namespace QuantumCore.Networking
{
    /// <summary>
    /// Marks the packet to be a sub packet, if the position is not 0 all fields before the position
    /// HAVE to match with all other packets of the same header!
    /// </summary>
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class SubPacketAttribute : Attribute
    {
        public byte SubHeader { get; set; }
        public int Position { get; set; }
        
        public SubPacketAttribute(byte subHeader, int position)
        {
            SubHeader = subHeader;
            Position = position;
        }
    }
}