using System;
using JetBrains.Annotations;

namespace QuantumCore.Core.Networking
{
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class ListenerAttribute : Attribute
    {
        public Type Packet { get; private set; }

        public ListenerAttribute(Type packet)
        {
            Packet = packet;
        }
    }
}