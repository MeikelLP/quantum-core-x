namespace QuantumCore.API.Game.Guild;

[Flags]
public enum GuildRankPermission : byte
{
    None = 0,
    AddMember = 1,
    RemoveMember = 2,
    ModifyNews = 4,
    UseSkill = 8,
    All = AddMember | RemoveMember | ModifyNews | UseSkill
}