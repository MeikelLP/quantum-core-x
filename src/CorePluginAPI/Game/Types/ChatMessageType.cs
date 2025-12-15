namespace QuantumCore.API.Game.Types;

public enum ChatMessageType : byte
{
    NORMAL,
    INFO,

    // What is type 2?
    GROUP = 3,
    GUILD,
    COMMAND,
    SHOUT
}
