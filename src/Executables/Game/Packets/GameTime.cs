using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x6a)]
public readonly ref partial struct GameTime
{
    public readonly uint Time;

    public GameTime(uint time)
    {
        Time = time;
    }
}