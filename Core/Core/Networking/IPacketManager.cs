using System;
using QuantumCore.Core.Packets;

namespace QuantumCore.Core.Networking
{
    public interface IPacketManager
    {
        public bool IsRegisteredOutgoing(Type packet);
        public PacketCache GetOutgoingPacket(byte header);
        public PacketCache GetIncomingPacket(byte header);
    }
}