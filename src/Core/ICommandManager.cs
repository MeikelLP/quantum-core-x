using System.Reflection;
using QuantumCore.Game;
using QuantumCore.Game.World;

namespace QuantumCore;

public interface ICommandManager
{
    void Register(string ns, Assembly? assembly = null);
    Task ReloadAsync(CancellationToken token = default);
    bool HavePerm(Guid group, string cmd);
    bool CanUseCommand(IPlayerEntity player, string cmd);
    Task HandleAsync(IGameConnection connection, string chatline);
    Dictionary<Guid, PermissionGroup> Groups { get; }
}