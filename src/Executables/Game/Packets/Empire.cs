using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x5a, HasSequence = true)]
[ClientToServerPacket(0x5a, HasSequence = true)]
public readonly ref partial struct Empire
{
    public readonly byte EmpireId;

    public Empire(byte empireId)
    {
        EmpireId = empireId;
    }
}