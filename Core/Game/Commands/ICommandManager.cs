using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using QuantumCore.API;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands;

public interface ICommandManager
{
    void Register(string ns, Assembly assembly = null);
    Task LoadAsync(CancellationToken token = default);
    bool HavePerm(Guid group, string cmd);
    bool CanUseCommand(IPlayerEntity player, string cmd);
    Task Handle(IGameConnection connection, string chatline);
    Dictionary<Guid, PermissionGroup> Groups { get; }
}