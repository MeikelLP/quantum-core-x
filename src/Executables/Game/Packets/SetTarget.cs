using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x3f)]
public readonly ref partial struct SetTarget
{
    public readonly uint TargetVid;
    public readonly byte Percentage;

    public SetTarget(uint targetVid, byte percentage)
    {
        TargetVid = targetVid;
        Percentage = percentage;
    }
}