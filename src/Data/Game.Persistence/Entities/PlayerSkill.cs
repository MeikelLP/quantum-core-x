using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuantumCore.API.Game.Skills;

namespace QuantumCore.Game.Persistence.Entities;

public class PlayerSkill
{
    public required Guid Id { get; set; } = Guid.NewGuid();
    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public required DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public required uint PlayerId { get; init; }
    public required uint SkillId { get; set; }
    public required uint ReadsRequired { get; set; } = 0;
    
    [DefaultValue(ESkillMasterType.Normal)] public required ESkillMasterType MasterType { get; set; }
    [DefaultValue(0)] public required byte Level { get; set; }
    [DefaultValue(0)] public required int NextReadTime { get; set; }
    
    public static void Configure(EntityTypeBuilder<PlayerSkill> builder, DatabaseFacade database)
    {
        builder.HasKey(x => new {x.Id});
        
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
    }
}
