using QuantumCore.API.Core.Models;
using QuantumCore.Game.Persistence.Entities;

namespace QuantumCore.Game.Persistence.Extensions;

internal static class DataExtensions
{
    internal static PlayerDeleted ToPlayerDeleted(this PlayerData p)
    {
        return new PlayerDeleted
        {
            AccountId = p.AccountId,
            Name = p.Name,
            PlayerClass = p.PlayerClass,
            SkillGroup = p.SkillGroup,
            PlayTime = p.PlayTime,
            Level = p.Level,
            Experience = p.Experience,
            Gold = p.Gold,
            St = p.St,
            Ht = p.Ht,
            Dx = p.Dx,
            Iq = p.Iq,
            PositionX = p.PositionX,
            PositionY = p.PositionY,
            Health = p.Health,
            Mana = p.Mana,
            Stamina = p.Stamina,
            BodyPart = p.BodyPart,
            HairPart = p.HairPart,
            Id = p.Id
        };
    }
}