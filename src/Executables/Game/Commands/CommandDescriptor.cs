namespace QuantumCore.Game.Commands;

internal record CommandDescriptor(Type Type, string Command, string? Description = null,
    Type? OptionsType = null, bool BypassPerm = false);
