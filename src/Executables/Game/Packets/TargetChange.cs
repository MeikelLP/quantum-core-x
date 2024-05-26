using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ClientToServerPacket(0x3d, HasSequence = true)]
public readonly ref partial struct TargetChange
{
    public readonly uint TargetVid;

    public TargetChange(uint targetVid)
    {
        TargetVid = targetVid;
    }
}