using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x79)]
public readonly ref partial struct Channel
{
    public readonly byte ChannelNo;

    public Channel(byte channelNo)
    {
        ChannelNo = channelNo;
    }
}