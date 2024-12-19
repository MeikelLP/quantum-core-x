using Microsoft.EntityFrameworkCore;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.Game.Persistence.Entities;
using QuantumCore.Game.Persistence.Extensions;

namespace QuantumCore.Game.Persistence;

public class DbPlayerRepository : IDbPlayerRepository
{
    private readonly GameDbContext _db;

    public DbPlayerRepository(GameDbContext db)
    {
        _db = db;
    }

    public async Task<PlayerData[]> GetPlayersAsync(Guid accountId)
    {
        return await _db.Players
            .AsNoTracking()
            .Where(x => x.AccountId == accountId)
            .SelectPlayerData()
            .ToArrayAsync();
    }

    public async Task<bool> IsNameInUseAsync(string name)
    {
        return await _db.Players.AnyAsync(x => x.Name == name);
    }

    public async Task CreateAsync(PlayerData player)
    {
        var entity = new Player
        {
            Id = player.Id,
            AccountId = player.AccountId,
            Name = player.Name,
            PlayerClass = (byte)player.PlayerClass,
            SkillGroup = player.SkillGroup,
            PlayTime = player.PlayTime,
            Level = player.Level,
            Experience = player.Experience,
            Gold = player.Gold,
            St = player.St,
            Ht = player.Ht,
            Dx = player.Dx,
            Iq = player.Iq,
            PositionX = player.PositionX,
            PositionY = player.PositionY,
            Health = player.Health,
            Mana = player.Mana,
            Stamina = player.Stamina,
            BodyPart = player.BodyPart,
            HairPart = player.HairPart,
            GivenStatusPoints = player.GivenStatusPoints,
            AvailableStatusPoints = player.AvailableStatusPoints,
            AvailableSkillPoints = player.AvailableSkillPoints,
            Empire = player.Empire,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Add(entity);
        await _db.SaveChangesAsync();
        player.Id = entity.Id;
    }

    public async Task DeletePlayerAsync(PlayerData player)
    {
        await _db.Players.Where(x => x.Id == player.Id).ExecuteDeleteAsync();
    }

    public async Task UpdateEmpireAsync(Guid accountId, uint playerId, EEmpire empire)
    {
        await _db.Players
            .Where(x => x.AccountId == accountId && x.Id == playerId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Empire, empire));
    }

    public async Task SetPlayerAsync(PlayerData data)
    {
        var entity = await _db.Players.FirstOrDefaultAsync(x => x.Id == data.Id);
        if (entity is null) return;

        entity.Empire = data.Empire;
        entity.PlayerClass = (byte)data.PlayerClass;
        entity.SkillGroup = data.SkillGroup;
        entity.PlayTime = data.PlayTime;
        entity.Level = data.Level;
        entity.Experience = data.Experience;
        entity.Gold = data.Gold;
        entity.St = data.St;
        entity.Ht = data.Ht;
        entity.Dx = data.Dx;
        entity.Iq = data.Iq;
        entity.PositionX = data.PositionX;
        entity.PositionY = data.PositionY;
        entity.Health = data.Health;
        entity.Mana = data.Mana;
        entity.Stamina = data.Stamina;
        entity.BodyPart = data.BodyPart;
        entity.HairPart = data.HairPart;
        entity.Name = data.Name;
        entity.GivenStatusPoints = data.GivenStatusPoints;
        entity.AvailableStatusPoints = data.AvailableStatusPoints;
        entity.AvailableSkillPoints = data.AvailableSkillPoints;

        await _db.SaveChangesAsync();
    }

    public async Task<PlayerData?> GetPlayerAsync(uint playerId)
    {
        return await _db.Players
            .Where(x => x.Id == playerId)
            .SelectPlayerData()
            .FirstOrDefaultAsync();
    }
}
