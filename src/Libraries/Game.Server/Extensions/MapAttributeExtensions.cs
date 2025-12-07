using System.Numerics;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Game.World;

namespace QuantumCore.Game.Extensions;

internal static class MapAttributeExtensions
{
    public static bool PositionIsAttr(this IEntity entity, EMapAttributes flags)
    {
        return entity.Map is Map localMap && localMap.IsAttr(entity.Coordinates(), flags);
    }
    
    // TODO: optimize / improve this function for better AI following around obstacles
    public static bool IsAttrOnStraightPathTo(this IEntity entity, Coordinates endCoords, EMapAttributes flags)
    {
        if (entity.Map is not Map localMap)
            return false;

        // cast to int since delta might be negative
        var dx = (int)endCoords.X - (int)entity.Coordinates().X;
        var dy = (int)endCoords.Y - (int)entity.Coordinates().Y;
        if (dx == 0 && dy == 0)
        {
            return entity.PositionIsAttr(flags);
        }

        const int Samples = 100;
        for (var i = 1; i <= Samples; i++)
        {
            var t = (float)i / Samples;
            var sampleDelta = new Vector2(dx * t, dy * t);

            if (localMap.IsAttr(entity.Coordinates() + sampleDelta, flags))
            {
                return true;
            }
        }

        return false;
    }
    
    public static Coordinates Coordinates(this IEntity entity)
    {
        // TODO: entity position should be directly refactored as Coordinate instead?
        return new Coordinates((uint)entity.PositionX, (uint)entity.PositionY);
    }
}
