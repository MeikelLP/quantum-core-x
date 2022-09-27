using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.Game.Constants;

namespace QuantumCore.Game.Extensions;

public static class ItemExtensions
{
    public static bool TryGetItemUseApplyType(this ItemData itemProto, out EEffectType value)
    {
        var type = (EApplyType) itemProto.Values[0];
        return ItemConstants.ApplyTypeToApplyPointMapping.TryGetValue(type, out value);
    }
}