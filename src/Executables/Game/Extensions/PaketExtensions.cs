using QuantumCore.API.Core.Models;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.Extensions;

public static class PaketExtensions
{
    public static Character ToCharacter(this PlayerData player, int ip = 0, ushort port = 0)
    {
        return new Character
        (
            1,
            player.Name,
            player.PlayerClass,
            player.Level,
            player.PlayTime,
            player.St,
            player.Ht,
            player.Dx,
            player.Iq,
            (ushort) player.BodyPart,
            0,
            (ushort) player.HairPart,
            0,
            player.PositionX,
            player.PositionY,
            ip,
            port,
            player.SkillGroup
        );
    }
}