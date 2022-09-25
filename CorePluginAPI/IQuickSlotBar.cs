using System.Threading.Tasks;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;

namespace QuantumCore.API;

public interface IQuickSlotBar
{
    IPlayerEntity Player { get; }
    QuickSlotData[] Slots { get; }
    Task Load();
    Task Persist();
    Task Send();
    Task Add(byte position, QuickSlotData slot);
    Task Swap(byte position1, byte position2);
    Task Remove(byte position);
}