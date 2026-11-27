namespace QuantumCore.API.Game.Types.Guild;

[Flags]
public enum GuildRankPermissions : byte
{
    NONE = 0,
    ADD_MEMBER = 1,
    REMOVE_MEMBER = 2,
    MODIFY_NEWS = 4,
    USE_SKILL = 8,
    ALL = ADD_MEMBER | REMOVE_MEMBER | MODIFY_NEWS | USE_SKILL
}
