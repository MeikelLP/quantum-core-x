using System.Collections.Generic;
using QuantumCore.API.Game.Types;

namespace QuantumCore.Game.Extensions;

public static class EffectExtensions
{
    public static EAffectFlags GetAffectFlags(this IEnumerable<EffectData> effects)
    {
        var flag = (EAffectFlags)0;
        foreach (var (_, flags, _, _) in effects)
        {
            flag |= flags;
        }

        return flag;
    }

    public static EAffectFlags GetAffectFlags(this IDictionary<EEffectType, EffectData> effects) 
        => effects.Values.GetAffectFlags();
}