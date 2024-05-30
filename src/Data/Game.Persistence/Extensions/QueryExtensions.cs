using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Skills;
using QuantumCore.Game.Persistence.Entities;

namespace QuantumCore.Game.Persistence.Extensions;

public static class QueryExtensions
{
    public static IQueryable<PlayerData> SelectPlayerData(this IQueryable<Player> query)
    {
        return query.Select(x => new PlayerData
        {
            Id = x.Id,
            AccountId = x.AccountId,
            Name = x.Name,
            PlayerClass = x.PlayerClass,
            SkillGroup = x.SkillGroup,
            PlayTime = x.PlayTime,
            Level = x.Level,
            Experience = x.Experience,
            Gold = x.Gold,
            St = x.St,
            Ht = x.Ht,
            Dx = x.Dx,
            Iq = x.Iq,
            PositionX = x.PositionX,
            PositionY = x.PositionY,
            Health = x.Health,
            Mana = x.Mana,
            Stamina = x.Stamina,
            BodyPart = x.BodyPart,
            HairPart = x.HairPart,
            GivenStatusPoints = x.GivenStatusPoints,
            AvailableStatusPoints = x.AvailableStatusPoints,
            AvailableSkillPoints = x.AvailableSkillPoints,
            Empire = x.Empire
        });
    }
    
    public static IQueryable<SkillData> SelectSkillData(this IQueryable<SkillProto> query)
    {
        return query.Select(x => new SkillData
        {
            Id = x.Id,
            Name = x.Name,
            Type = x.Type,
            LevelStep = x.LevelStep,
            MaxLevel = x.MaxLevel,
            LevelLimit = x.LevelLimit,
            PointOn = x.PointOn,
            PointPoly = x.PointPoly,
            SPCostPoly = x.SPCostPoly,
            DurationPoly = x.DurationPoly,
            DurationSPCostPoly = x.DurationSPCostPoly,
            CooldownPoly = x.CooldownPoly,
            MasterBonusPoly = x.MasterBonusPoly,
            AttackGradePoly = x.AttackGradePoly,
            Flags = x.Flags,
            AffectFlags = x.AffectFlags,
            PointOn2 = x.PointOn2,
            PointPoly2 = x.PointPoly2,
            DurationPoly2 = x.DurationPoly2,
            AffectFlags2 = x.AffectFlags2,
            PointOn3 = x.PointOn3,
            PointPoly3 = x.PointPoly3,
            DurationPoly3 = x.DurationPoly3,
            GrandMasterAddSPCostPoly = x.GrandMasterAddSPCostPoly,
            PrerequisiteSkillVnum = x.PrerequisiteSkillVnum,
            PrerequisiteSkillLevel = x.PrerequisiteSkillLevel,
            SkillType = x.SkillType,
            MaxHit = x.MaxHit,
            SplashAroundDamageAdjustPoly = x.SplashAroundDamageAdjustPoly,
            TargetRange = x.TargetRange,
            SplashRange = x.SplashRange
        });
    }
    
    public static IQueryable<IPlayerSkill> SelectPlayerSkill(this IQueryable<PlayerSkill> query)
    {
        return query.Select(x => new PlayerSkill
        {
            PlayerId = x.PlayerId,
            SkillId = x.SkillId,
            MasterType = x.MasterType,
            Level = x.Level,
            NextReadTime = x.NextReadTime,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        });
        
    }

    public static IQueryable<ItemInstance> SelectInstance(this IQueryable<Item> query)
    {
        return query.Select(x => new ItemInstance
        {
            Id = x.Id,
            PlayerId = x.PlayerId,
            ItemId = x.ItemId,
            Window = x.Window,
            Position = x.Position,
            Count = x.Count
        });
    }
}
