using QuantumCore.API;
using QuantumCore.API.Game.Guild;
using QuantumCore.API.Game.Types;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets.Guild;

namespace QuantumCore.Game.PacketHandlers.Game.Guild;

public class GuildExperienceInvestHandler : IGamePacketHandler<GuildExperienceInvestPacket>
{
    private readonly IGuildManager _guildManager;

    public GuildExperienceInvestHandler(IGuildManager guildManager)
    {
        _guildManager = guildManager;
    }

    public async Task ExecuteAsync(GamePacketContext<GuildExperienceInvestPacket> ctx,
        CancellationToken token = default)
    {
        var player = ctx.Connection.Player!;
        if (player.Player.GuildId is null)
        {
            player.SendChatInfo("You are not in a guild.");
            return;
        }

        var guild = await _guildManager.GetGuildByIdAsync(player.Player.GuildId.Value, token);
        if (guild!.Level == GuildConstants.MAX_LEVEL)
        {
            player.SendChatInfo("Guild is already at max level.");
            return;
        }

        if (player.Player.Experience < ctx.Packet.Amount)
        {
            player.SendChatInfo("You don't have enough experience.");
            return;
        }

        var spenderId = player.Player.Id;
        var amount = ctx.Packet.Amount / 100 * 100; // round to 100s
        guild = await _guildManager.AddExperienceAsync(spenderId, amount, token);
        player.SetPoint(EPoint.Experience, player.Player.Experience - amount);
        player.SendPoints();

        foreach (var p in player.Map!.World.GetGuildMembers(guild.Id))
        {
            p.Connection.SendGuildInfo(guild);
        }
    }
}