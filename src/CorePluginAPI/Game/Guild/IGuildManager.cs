﻿using System.Collections.Immutable;

namespace QuantumCore.API.Game.Guild;

/// <summary>
/// This manager does not validate input or overflows. Please validate yourself and updating using this manager.
/// Does not send any packets. Used to update the database
/// </summary>
public interface IGuildManager
{
    Task<GuildData?> GetGuildByNameAsync(string name, CancellationToken token = default);
    Task<GuildData?> GetGuildByIdAsync(uint id, CancellationToken token = default);
    Task<GuildData?> GetGuildForPlayerAsync(uint playerId, CancellationToken token = default);
    Task<GuildData> CreateGuildAsync(string name, uint leaderId, CancellationToken token = default);
    Task<uint> CreateNewsAsync(uint guildId, string message, uint playerId, CancellationToken token = default);
    Task<bool> HasPermissionAsync(uint playerId, GuildRankPermission perm, CancellationToken token = default);
    Task<ImmutableArray<GuildNewsData>> GetGuildNewsAsync(uint guildId, CancellationToken token = default);
    Task<bool> DeleteNewsAsync(uint guildId, uint newsId, CancellationToken token = default);
    Task<bool> IsLeaderAsync(uint playerId, CancellationToken token = default);
    Task<ImmutableArray<GuildRankData>> GetRanksAsync(uint guildId, CancellationToken token = default);
    Task RenameRankAsync(uint guildId, byte position, string packetName, CancellationToken token = default);
    Task RemoveGuildAsync(uint guildId, CancellationToken token = default);

    Task ChangePermissionAsync(uint guildId, byte position, GuildRankPermission permissions,
        CancellationToken token = default);

    Task AddMemberAsync(uint guildId, uint inviteeId, byte rank, CancellationToken token = default);
    Task RemoveMemberAsync(uint playerId, CancellationToken token = default);
    Task SetLeaderAsync(uint playerId, bool toggle, CancellationToken token = default);

    /// <summary>
    /// Checks for level up and raises the guild level accordingly
    /// </summary>
    /// <returns>Simple data of the updated guild to send to the clients</returns>
    Task<GuildData> AddExperienceAsync(uint spenderId, uint amount, CancellationToken token = default);
}
