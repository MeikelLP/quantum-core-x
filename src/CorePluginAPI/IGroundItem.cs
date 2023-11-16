﻿using System.Threading.Tasks;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;

namespace QuantumCore.API;

public interface IGroundItem : IEntity
{
    ItemInstance Item { get; }
    uint Amount { get; }
}
