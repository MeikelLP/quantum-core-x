using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ClientToServerPacket(0xf1, HasSequence = true)]
public readonly ref partial struct Version
{
    [FixedSizeString(33)] public readonly string ExecutableName;
    [FixedSizeString(33)] public readonly string Timestamp;

    public Version(string executableName, string timestamp)
    {
        ExecutableName = executableName;
        Timestamp = timestamp;
    }
}