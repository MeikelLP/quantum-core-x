using System.Collections.Immutable;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;

namespace QuantumCore.API;

public interface IQuickSlotBar
{
    IPlayerEntity Player { get; }
    ImmutableArray<QuickSlotData?> Slots { get; }
    Task LoadAsync();
    Task PersistAsync();
    void Send();
    void Add(byte position, QuickSlotData slot);
    void Swap(byte position1, byte position2);
    void Remove(byte position);
}