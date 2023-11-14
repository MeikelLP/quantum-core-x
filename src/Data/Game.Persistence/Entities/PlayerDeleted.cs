﻿using System.ComponentModel.DataAnnotations.Schema;
using Core.Persistence;

namespace QuantumCore.Game.Persistence.Entities;

[Table("deleted_players")]
internal class PlayerDeleted : BaseModel
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
}