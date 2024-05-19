using System.Reflection;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;

namespace QuantumCore.API;

public interface ICommandManager
{
    void Register(string ns, Assembly? assembly = null);
    Task LoadAsync(CancellationToken token = default);
    Task ReloadAsync(CancellationToken token = default);
    bool HavePerm(Guid group, string cmd);
    bool CanUseCommand(IPlayerEntity player, string cmd);
    Task Handle(IGameConnection connection, string chatline);
    Dictionary<Guid, PermissionGroup> Groups { get; }
}