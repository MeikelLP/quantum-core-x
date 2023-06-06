using System;
using System.Collections.Generic;
using System.Reflection;

namespace QuantumCore.Core.Networking
{
    public interface IPacketManager
    {
        public bool IsRegisteredOutgoing(Type packet);
        public PacketCache GetOutgoingPacket(ushort header);
        public PacketCache GetIncomingPacket(ushort header);
        void RegisterNamespace(string space, Assembly assembly = null);
        void Register<T>();
        Dictionary<ushort, PacketCache> OutgoingPackets { get; }
        Dictionary<ushort, PacketCache> IncomingPackets { get; }
        IPacketCache GetPacket<T>();
        IPacketCache GetPacket(Type type);
    }
}