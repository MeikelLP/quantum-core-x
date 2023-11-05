using System.Data;
using Dapper;
using QuantumCore.API.Data;

namespace QuantumCore.Database.Repositories;

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
        return await _db.QueryAsync<Guid>("SELECT Player FROM perm_users WHERE `Group` = @Group", new { Group = groupId });
    }

    public async Task<IEnumerable<(Guid Id, string Name)>> GetGroupsAsync()
    {
        return await _db.QueryAsync<(Guid Id, string Name)>("SELECT Id, Name FROM perm_groups");
    }
}
