namespace QuantumCore.API.Game.Types;

public record struct EffectData(EEffectType Type = default, EAffectFlags Flags = default, uint Duration = default, short Value = default);