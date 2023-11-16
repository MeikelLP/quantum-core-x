using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;

namespace QuantumCore.API;

public interface IQuickSlotBar
{
    IPlayerEntity Player { get; }
    QuickSlotData?[] Slots { get; }
    Task Load();
    Task Persist();
    void Send();
    void Add(byte position, QuickSlotData slot);
    void Swap(byte position1, byte position2);
    void Remove(byte position);
}
