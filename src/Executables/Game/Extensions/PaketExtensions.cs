using QuantumCore.API.Core.Models;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.Extensions;

public static class PaketExtensions
{
    public static Character ToCharacter(this PlayerData player)
    {
        return new Character
        {
            Id = 1,
            Name = player.Name,
            Class = player.PlayerClass,
            Level = player.Level,
            Playtime = player.PlayTime == 0 ? 0 : (uint) player.PlayTime / 60000, // Milliseconds to minutes
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
