namespace QuantumCore.API.Game.Types;

public enum ChatMessageType : byte
{
    Normal,
    Info,

    // What is type 2?
    Group = 3,
    Guild,
    Command,
    Shout
}
