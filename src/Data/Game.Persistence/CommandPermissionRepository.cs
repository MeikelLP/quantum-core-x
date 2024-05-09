using Microsoft.EntityFrameworkCore;
using QuantumCore.API.Game;

namespace QuantumCore.Game.Persistence;

public interface ICommandPermissionRepository
{
    Task<IEnumerable<string>> GetPermissionsForGroupAsync(Guid groupId);
    Task<IEnumerable<Guid>> GetPlayerIdsInGroupAsync(Guid groupId);
    Task<IEnumerable<PermissionGroup>> GetGroupsAsync();
}

public class CommandPermissionRepository : ICommandPermissionRepository
{
    private readonly GameDbContext _db;

    public CommandPermissionRepository(GameDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<string>> GetPermissionsForGroupAsync(Guid groupId)
    {
        return await _db.Permissions
            .Where(x => x.GroupId == groupId)
            .Select(x => x.Command)
            .ToArrayAsync();
    }

    public async Task<IEnumerable<Guid>> GetPlayerIdsInGroupAsync(Guid groupId)
    {
        return await _db.PermissionUsers
            .Where(x => x.GroupId == groupId)
            .Select(x => x.PlayerId)
            .ToArrayAsync();
    }

    public async Task<IEnumerable<PermissionGroup>> GetGroupsAsync()
    {
        return await _db.PermissionGroups
            .Select(x => new PermissionGroup
            {
                Id = x.Id,
                Name = x.Name,
                Permissions = x.Permissions.Select(p => p.Command).ToList(),
                Users = x.Users.Select(u => u.PlayerId).ToList()
            })
            .ToArrayAsync();
    }
}