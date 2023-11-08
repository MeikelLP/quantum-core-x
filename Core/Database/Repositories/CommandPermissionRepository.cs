using System.Data;
using Dapper;

namespace QuantumCore.Database.Repositories;

public interface ICommandPermissionRepository
{
    Task<IEnumerable<string>> GetPermissionsForGroupAsync(Guid groupId);
    Task<IEnumerable<Guid>> GetPlayerIdsInGroupAsync(Guid groupId);
    Task<IEnumerable<(Guid Id, string Name)>> GetGroupsAsync();
}

public class CommandPermissionRepository : ICommandPermissionRepository
{
    private readonly IDbConnection _db;

    public CommandPermissionRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<IEnumerable<string>> GetPermissionsForGroupAsync(Guid groupId)
    {
        return await _db.QueryAsync<string>("SELECT Command FROM perm_auth WHERE `Group` = @Group", new { Group = groupId });
    }

    public async Task<IEnumerable<Guid>> GetPlayerIdsInGroupAsync(Guid groupId)
    {
        var results = await _db.QueryAsync<string>("SELECT Player FROM perm_users WHERE `Group` = @Group", new { Group = groupId });
        return results.Select(Guid.Parse);
    }

    public async Task<IEnumerable<(Guid Id, string Name)>> GetGroupsAsync()
    {
        var results = await _db.QueryAsync<(string Id, string Name)>("SELECT Id, Name FROM perm_groups");
        return results.Select(x => (Guid.Parse(x.Id), x.Name));
    }
}
