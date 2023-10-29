using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using QuantumCore.API.Core.Models;

namespace QuantumCore.API;

public interface IItemManager
{
    ItemData GetItem(uint id);
    [CanBeNull] ItemData GetItemByName(ReadOnlySpan<char> name);
    Task LoadAsync(CancellationToken token = default);
    ItemInstance CreateItem(ItemData proto, byte count = 1);
}
