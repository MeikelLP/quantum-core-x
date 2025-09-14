using QuantumCore.API.Core.Models;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.Extensions;

public static class PaketExtensions
{
    public static Character ToCharacter(this PlayerData player)
    {
        return new Character
        {
            Id = player.Id,
            Name = player.Name,
            Class = player.PlayerClass,
            Level = player.Level,
            Playtime = (uint) TimeSpan.FromMilliseconds(player.PlayTime).TotalMinutes,
            St = player.St,
            Ht = player.Ht,
            Dx = player.Dx,
            Iq = player.Iq,
            BodyPart = (ushort) player.BodyPart,
            NameChange = 0,
            HairPort = (ushort) player.HairPart,
            PositionX = player.PositionX,
            PositionY = player.PositionY,
            SkillGroup = player.SkillGroup
        };
    }
}
