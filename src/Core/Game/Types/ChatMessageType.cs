namespace QuantumCore.API.Game.Types;

public enum ChatMessageType : byte
{
    NORMAL = 0,
    INFO = 1,
    NOTICE = 2,
    GROUP = 3,
    GUILD = 4,
    COMMAND = 5,
    SHOUT = 6,
    WHISPER = 7, // only used on the client
    BIG_NOTICE = 8
}