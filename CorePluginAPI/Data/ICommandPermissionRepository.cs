namespace QuantumCore.API.Data;

public interface ICommandPermissionRepository
{
    Task<IEnumerable<string>> GetPermissionsForGroupAsync(Guid groupId);
    Task<IEnumerable<Guid>> GetPlayerIdsInGroupAsync(Guid groupId);
    Task<IEnumerable<(Guid Id, string Name)>> GetGroupsAsync();
}
