namespace QuantumCore.API.PluginTypes;

public interface IPacketHandler
{
}

public interface IGamePacketHandler<T> : IPacketHandler
{
    Task ExecuteAsync(GamePacketContext<T> ctx, CancellationToken token = default);
}

public interface IAuthPacketHandler<T> : IPacketHandler
{
    Task ExecuteAsync(AuthPacketContext<T> ctx, CancellationToken token = default);
}