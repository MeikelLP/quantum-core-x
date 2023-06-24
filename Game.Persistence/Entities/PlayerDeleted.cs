using System.ComponentModel.DataAnnotations.Schema;
using Core.Persistence;

namespace QuantumCore.Game.Persistence.Entities;

[Table("deleted_players")]
public class PlayerDeleted : BaseModel
{
    public Guid AccountId { get; set; }
    public string Name { get; set; }
    public byte PlayerClass { get; set; }
    public byte SkillGroup { get; set; }
    public uint PlayTime { get; set; }
    public byte Level { get; set; } = 1;
    public uint Experience { get; set; }
    public uint Gold { get; set; }
    public byte St { get; set; }
    public byte Ht { get; set; }
    public byte Dx { get; set; }
    public byte Iq { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
    public long Health { get; set; }
    public long Mana { get; set; }
    public long Stamina { get; set; }
    public uint BodyPart { get; set; }
    public uint HairPart { get; set; }

    public DateTime DeletedAt { get; set; } = DateTime.Now;

    public PlayerDeleted(Player p)
    {
        AccountId = p.AccountId;
        Name = p.Name;
        PlayerClass = p.PlayerClass;
        SkillGroup = p.SkillGroup;
        PlayTime = p.PlayTime;
        Level = p.Level;
        Experience = p.Experience;
        Gold = p.Gold;
        St = p.St;
        Ht = p.Ht;
        Dx = p.Dx;
        Iq = p.Iq;
        PositionX = p.PositionX;
        PositionY = p.PositionY;
        Health = p.Health;
        Mana = p.Mana;
        Stamina = p.Stamina;
        BodyPart = p.BodyPart;
        HairPart = p.HairPart;
        Id = p.Id;
        CreatedAt = p.CreatedAt;
        UpdatedAt = p.UpdatedAt;
    }
}