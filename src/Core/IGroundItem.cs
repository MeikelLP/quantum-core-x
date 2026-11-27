using QuantumCore.Game.World;
using QuantumCore.Models;

namespace QuantumCore;

public interface IGroundItem : IEntity
{
    ItemInstance Item { get; }
    uint Amount { get; }
    string? OwnerName { get; }
}
