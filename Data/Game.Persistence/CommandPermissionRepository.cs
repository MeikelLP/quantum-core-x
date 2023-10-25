using System.Data;
using Dapper;

namespace QuantumCore.Game.Persistence;

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
        // for some reason the Id cannot be selected as GUID thus we have to convert on it client side
        var ids = await _db.QueryAsync<string>("SELECT Player FROM perm_users WHERE `Group` = @Group", new { Group = groupId });
        return ids.Select(Guid.Parse).ToArray();
    }

    public async Task<IEnumerable<(Guid Id, string Name)>> GetGroupsAsync()
    {
        // for some reason the Id cannot be selected as GUID thus we have to convert on it client side
        var values = await _db.QueryAsync<(string Id, string Name)>("SELECT Id, Name FROM perm_groups");
        return values.Select(x => (Guid.Parse(x.Id), x.Name)).ToArray();
    }
}
