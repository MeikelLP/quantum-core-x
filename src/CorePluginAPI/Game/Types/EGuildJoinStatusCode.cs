namespace QuantumCore.API.Game.Types;

public enum EGuildJoinStatusCode : byte
{
    Success = 0,
    AlreadyInAnyGuild = 1,
    GuildFull = 2
}