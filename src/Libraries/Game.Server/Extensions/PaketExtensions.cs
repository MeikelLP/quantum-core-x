using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types.Players;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Packets;
using QuantumCore.Networking;

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
            NameChange = ENameChangeStatus.DISABLED,
            HairPart = (ushort) player.HairPart,
            PositionX = player.PositionX,
            PositionY = player.PositionY,
            SkillGroup = player.SkillGroup
        };
    }
    
    public static void SafeBroadcastNearby<T>(this IEntity entity, T packet, bool includeSelf = true) where T : IPacketSerializable
    {
        if (includeSelf && entity is IPlayerEntity player)
        {
            player.Connection.Send(packet);
        }

        // take a snapshot to avoid enumeration failure if the nearby list is being mutated while we send
        var nearbySnapshot = entity.NearbyEntities.AsEnumerable().ToArray();

        foreach (var nearbyPlayer in nearbySnapshot
                     .Where(x => x is IPlayerEntity)
                     .Cast<IPlayerEntity>())
        {
            nearbyPlayer.Connection.Send(packet);
        }
    }
}
