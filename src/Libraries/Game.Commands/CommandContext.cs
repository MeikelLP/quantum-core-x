using QuantumCore.API.Game.World;

public record struct CommandContext<T>(IPlayerEntity Player, T Arguments);

public record struct CommandContext(IPlayerEntity Player);