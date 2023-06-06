using QuantumCore.Core.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0xCE, EDirection.Incoming)]
public class StateCheckPacket
{
}

[Packet(0xD2, EDirection.Outgoing)]
public class ServerStatusPacket
{
    [Field(0)]
    [Size]
    public uint Size { get; set; }
    
    [Field(1)]
    [Dynamic]
    public ServerStatus[] Statuses { get; set; }
    
    [Field(2)] public byte IsSuccess { get; set; }
}


public class ServerStatus
{
    [Field(0)]
    public short Port { get; set; }
    
    [Field(1)]
    public byte Status { get; set; }
}