using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x09)]
public readonly ref partial struct CreateCharacterFailure
{
    public readonly byte Error;

    public CreateCharacterFailure(byte error)
    {
        Error = error;
    }
}