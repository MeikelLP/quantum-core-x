﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QuantumCore.Game.Persistence.Entities;

public class Player
{
    public required uint Id { get; set; }
    public required Guid AccountId { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required DateTime UpdatedAt { get; set; }
    public required byte Empire { get; set; }
    public required byte PlayerClass { get; set; }
    public required byte SkillGroup { get; set; }
    [DefaultValue(0)] public required ulong PlayTime { get; set; }
    [DefaultValue(1)] public required byte Level { get; set; }
    [DefaultValue(0)] public required uint Experience { get; set; }
    [DefaultValue(0)] public required uint Gold { get; set; }
    [DefaultValue(0)] public required byte St { get; set; }
    [DefaultValue(0)] public required byte Ht { get; set; }
    [DefaultValue(0)] public required byte Dx { get; set; }
    [DefaultValue(0)] public required byte Iq { get; set; }
    public required int PositionX { get; set; }
    public required int PositionY { get; set; }
    public required long Health { get; set; }
    public required long Mana { get; set; }
    public required long Stamina { get; set; }
    [DefaultValue(0)] public required uint BodyPart { get; set; }
    [DefaultValue(0)] public required uint HairPart { get; set; }
    [StringLength(24)] public required string Name { get; set; }
    [DefaultValue(0)] public required uint GivenStatusPoints { get; set; }
    [DefaultValue(0)] public required uint AvailableStatusPoints { get; set; }
    [DefaultValue(0)] public required uint AvailableSkillPoints { get; set; }
    

    public static void Configure(EntityTypeBuilder<Player> builder, DatabaseFacade database)
    {
        if (database.IsSqlite() || database.IsNpgsql())
        {
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("current_timestamp");
            builder.Property(x => x.UpdatedAt).HasDefaultValueSql("current_timestamp");
        }
        else if (database.IsMySql())
        {
            builder.Property(x => x.CreatedAt).HasDefaultValueSql("(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))");
            builder.Property(x => x.UpdatedAt).HasDefaultValueSql("(CAST(CURRENT_TIMESTAMP AS DATETIME(6)))");
        }

        builder.HasData([
            new Player
            {
                AccountId = Guid.Parse("E34FD5AB-FB3B-428E-935B-7DB5BD08A3E5"),
                Name = "Admin",
                St = 99,
                Ht = 99,
                Dx = 99,
                Iq = 99,
                Health = 99_999,
                Mana = 99_999,
                Experience = 0,
                Level = 99,
                PlayerClass = 0,
                CreatedAt = new DateTime(2024,
                    1,
                    1,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc),
                Gold = 2_000_000_000,
                PositionX = 958870,
                PositionY = 272788,
                Id = 1,
                UpdatedAt = default,
                Empire = 0,
                SkillGroup = 0,
                PlayTime = 0,
                Stamina = 0,
                BodyPart = 0,
                HairPart = 0,
                GivenStatusPoints = 0,
                AvailableStatusPoints = 0,
                AvailableSkillPoints = 99
            }
        ]);
    }
}
