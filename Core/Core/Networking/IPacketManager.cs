using System;
using QuantumCore.Core.Packets;

namespace QuantumCore.Core.Networking
{
    public interface IPacketManager
    {
        public bool IsRegisteredOutgoing(Type packet);
        public PacketCache GetOutgoingPacket(ushort header);
        public PacketCache GetIncomingPacket(ushort header);
    }
}