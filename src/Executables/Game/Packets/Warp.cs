using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x41)]
public readonly ref partial struct Warp
{
    public readonly int PositionX;
    public readonly int PositionY;
    public readonly int ServerAddress;
    public readonly ushort ServerPort;

    public Warp(int positionX, int positionY, int serverAddress, ushort serverPort)
    {
        PositionX = positionX;
        PositionY = positionY;
        ServerAddress = serverAddress;
        ServerPort = serverPort;
    }
}