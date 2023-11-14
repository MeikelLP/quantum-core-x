using System.Collections.Immutable;
using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.Game.Persistence.Entities;

namespace QuantumCore.Game.Persistence;

public class AffectRepository : IAffectRepository
{
    private readonly IDbConnection _db;

    public AffectRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task RemoveAffectFromPlayerAsync(Guid playerId, EAffectType type, EApplyType applyOn)
    {
        await _db.QueryAsync("DELETE FROM affects WHERE PlayerId = @PlayerId and Type = @Type and ApplyOn = @ApplyOn",
            new { PlayerId = playerId, Type = type, ApplyOn = applyOn });
    }

    public async Task AddAffectAsync(Affect affectEntity)
    {
        await _db.InsertAsync(new AffectEntity
        {
            PlayerId = affectEntity.PlayerId,
            Type = affectEntity.Type,
            ApplyOn = affectEntity.ApplyOn,
            ApplyValue = affectEntity.ApplyValue,
            Flag = affectEntity.Flag,
            Duration = affectEntity.Duration,
            SpCost = affectEntity.SpCost
        });
    }

    public async Task<ImmutableArray<Affect>> GetAffectsForPlayerAsync(Guid playerId)
    {
        var result = await _db.QueryAsync<Affect>("SELECT * FROM affects WHERE PlayerId = @PlayerId",
            new { PlayerId = playerId });
        return result.ToImmutableArray();
    }
}
