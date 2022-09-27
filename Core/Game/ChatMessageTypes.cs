namespace QuantumCore.Game;

public enum ChatMessageTypes : byte
{
    Normal,
    Info,
    // What is type 2?
    Group = 3,
    Guild,
    Command,
    Shout,
};