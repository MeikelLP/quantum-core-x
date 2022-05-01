using System.Collections.Generic;
using System.Net;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.World;

public class RemoteMap : IMap
{
    public string Name { get; }
    public uint PositionX { get; }
    public uint UnitX => PositionX / Map.MapUnit;
    public uint PositionY { get; }
    public uint UnitY => PositionY / Map.MapUnit;
    public uint Width { get; }
    public uint Height { get; }
    
    public IPAddress Host { get; set; }
    public ushort Port { get; set; }

    public RemoteMap(string name, uint x, uint y, uint width, uint height)
    {
        Name = name;
        PositionX = x;
        PositionY = y;
        Width = width;
        Height = height;
    }
    
    public List<IEntity> GetEntities()
    {
        throw new System.NotImplementedException();
    }

    public IEntity GetEntity(uint vid)
    {
        throw new System.NotImplementedException();
    }

    public bool SpawnEntity(IEntity entity)
    {
        throw new System.NotImplementedException();
    }

    public void DespawnEntity(IEntity entity)
    {
        throw new System.NotImplementedException();
    }

    public bool IsPositionInside(int x, int y)
    {
        return x >= PositionX && x < PositionX + Width * Map.MapUnit && y >= PositionY && y < PositionY + Height * Map.MapUnit;
    }

    public void Update(double elapsedTime)
    {
        
    }
}