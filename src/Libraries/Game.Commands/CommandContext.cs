using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands;

public record struct CommandContext<T>(IPlayerEntity Player, T Arguments);

public record struct CommandContext(IPlayerEntity Player);
