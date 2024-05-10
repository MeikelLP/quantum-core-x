using Microsoft.EntityFrameworkCore;
using QuantumCore.API.Core.Models;
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
        _db.Add(new Player
        {
            Id = player.Id,
            AccountId = player.AccountId,
            Name = player.Name,
            PlayerClass = player.PlayerClass,
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
            Empire = player.Empire,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    public async Task DeletePlayerAsync(PlayerData player)
    {
        await _db.Players.Where(x => x.Id == player.Id).ExecuteDeleteAsync();
    }

    public async Task UpdateEmpireAsync(Guid accountId, Guid playerId, byte empire)
    {
        await _db.Players
            .Where(x => x.AccountId == accountId && x.Id == playerId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Empire, empire));
    }

    public async Task<PlayerData?> GetPlayerAsync(Guid playerId)
    {
        return await _db.Players
            .Where(x => x.Id == playerId)
            .SelectPlayerData()
            .FirstOrDefaultAsync();
    }
}