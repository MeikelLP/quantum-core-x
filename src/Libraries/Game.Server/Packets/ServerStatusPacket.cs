﻿using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0xD2, EDirection.Outgoing)]
[PacketGenerator]
public partial class ServerStatusPacket
{
    [Field(0)] public uint Size => (uint) Statuses.Length;

    [Field(1)] public ServerStatus[] Statuses { get; set; } = Array.Empty<ServerStatus>();

    [Field(2)] public byte IsSuccess { get; set; }
}

public class ServerStatus
{
    [Field(0)] public short Port { get; set; }

    [Field(1)] public byte Status { get; set; }
}