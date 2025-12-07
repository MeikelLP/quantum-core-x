namespace QuantumCore.API.Game.Types.Guild;

[Flags]
public enum GuildRankPermissions : byte
{
    None = 0,
    AddMember = 1,
    RemoveMember = 2,
    ModifyNews = 4,
    UseSkill = 8,
    All = AddMember | RemoveMember | ModifyNews | UseSkill
}
