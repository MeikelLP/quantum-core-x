using QuantumCore.API.Game.World;

namespace QuantumCore.API;

public interface IChatManager
{
    void Talk(IEntity entity, string message);
    Task Shout(string message);
}