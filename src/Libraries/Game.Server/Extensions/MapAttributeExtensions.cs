using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.World;

namespace QuantumCore.Game.Extensions;

internal static class MapAttributeExtensions
{
    public static bool PositionIsAttr(this IEntity entity, EMapAttribute flags)
    {
        return entity.Map is Map localMap && localMap.IsAttr(entity.PositionX, entity.PositionY, flags);
    }
    
    // TODO: optimize / improve this function for better AI following around obstacles
    public static bool IsAttrOnStraightPathTo(this IEntity entity, int endX, int endY, EMapAttribute flags)
    {
        if (entity.Map is not Map localMap)
            return false;

        var startX = entity.PositionX;
        var startY = entity.PositionY;
        var dx = endX - startX;
        var dy = endY - startY;

        if (dx == 0 && dy == 0)
        {
            return localMap.IsAttr(startX, startY, flags);
        }

        const int Samples = 100;
        for (var i = 1; i <= Samples; i++)
        {
            var t = i / (double)Samples;
            var sampleX = startX + (int)Math.Round(dx * t);
            var sampleY = startY + (int)Math.Round(dy * t);

            if (localMap.IsAttr(sampleX, sampleY, flags))
            {
                return true;
            }
        }

        return false;
    }
}
