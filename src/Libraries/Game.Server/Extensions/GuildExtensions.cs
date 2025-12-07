using QuantumCore.API.Game.Guild;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.Types.Guild;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Extensions;

public static class GuildExtensions
{
    public static EGuildJoinStatusCode CanJoinGuild(this GuildData guild, IPlayerEntity invitee)
    {
        // TODO check if player has recently left any guild
        // TODO check if player has recently dissolved any guild
        if (invitee.Player.GuildId is not null)
        {
            return EGuildJoinStatusCode.AlreadyInAnyGuild;
        }

        if (guild.Members.Length >= guild.MaxMemberCount)
        {
            return EGuildJoinStatusCode.GuildFull;
        }

        return EGuildJoinStatusCode.Success;
    }

    // TODO cache
    public static IEnumerable<IPlayerEntity> GetGuildMembers(this IWorld world, uint guildId)
    {
        var allPlayers = world.GetPlayers();
        return allPlayers
            .Where(p => p.Player.GuildId == guildId);
    }
}