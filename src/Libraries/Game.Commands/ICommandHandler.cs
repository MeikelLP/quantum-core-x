namespace QuantumCore.Game.Commands;

public interface ICommandHandler<T>
{
    Task ExecuteAsync(CommandContext<T> context);
}

public interface ICommandHandler
{
    Task ExecuteAsync(CommandContext context);
}