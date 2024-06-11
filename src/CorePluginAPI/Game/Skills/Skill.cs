﻿using System.Diagnostics;

namespace QuantumCore.API.Game.Skills;

[DebuggerDisplay("Skill ({SkillId}) - MasterType: {MasterType}, Level: {Level}")]
public class Skill : ISKill
{
    public ESkillIndexes SkillId { get; set; }
    public uint PlayerId { get; set; }
    public ESkillMasterType MasterType { get; set; }
    public byte Level { get; set; }
    public int NextReadTime { get; set; }
    public uint ReadsRequired { get; set; }
}
