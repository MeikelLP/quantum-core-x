using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0xD2)]
public readonly ref partial struct ServerStatusPacket
{
    public readonly uint Size;
    public readonly ServerStatus[] Statuses;
    public readonly byte IsSuccess;

    public ServerStatusPacket(ServerStatus[] statuses, byte isSuccess)
    {
        Size = (uint) statuses.Length;
        Statuses = statuses;
        IsSuccess = isSuccess;
    }
}