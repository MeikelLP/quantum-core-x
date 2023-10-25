using JetBrains.Annotations;

namespace QuantumCore.Game.Commands;

internal record CommandDescriptor(Type Type, string Command, string Description = null, [property:CanBeNull] Type OptionsType = null, bool BypassPerm = false);