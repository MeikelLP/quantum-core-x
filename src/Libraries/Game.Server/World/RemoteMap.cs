using System.Net;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.World;

public class RemoteMap : IMap
{
    public string Name { get; }
    public uint UnitX => Position.X / Map.MAP_UNIT;
    public Coordinates Position { get; }
    public uint UnitY => Position.Y / Map.MAP_UNIT;
    public uint Width { get; }
    public uint Height { get; }
    public IWorld World { get; }
    public IReadOnlyCollection<IEntity> Entities => throw new NotImplementedException();
    public TownCoordinates? TownCoordinates => throw new NotImplementedException();

    public IPAddress? Host { get; set; }
    public ushort Port { get; set; }

    public RemoteMap(IWorld world, string name, Coordinates position, uint width, uint height)
    {
        World = world;
        Name = name;
        Position = position;
        Width = width;
        Height = height;
    }

    public List<IEntity> GetEntities()
    {
        throw new NotImplementedException();
    }

    public IEntity GetEntity(uint vid)
    {
        throw new NotImplementedException();
    }

    public void SpawnEntity(IEntity entity)
    {
        throw new NotImplementedException();
    }

    public void DespawnEntity(IEntity entity)
    {
        throw new NotImplementedException();
    }

    public bool IsPositionInside(int x, int y)
    {
        return x >= Position.X && x < Position.X + Width * Map.MAP_UNIT && y >= Position.Y &&
               y < Position.Y + Height * Map.MAP_UNIT;
    }

    public void Update(double elapsedTime)
    {
    }

    public void AddGroundItem(ItemInstance item, int x, int y, uint amount = 0, string? ownerName = null)
    {
        throw new NotImplementedException();
    }
}
