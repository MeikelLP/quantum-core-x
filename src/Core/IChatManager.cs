using QuantumCore.Game.World;

namespace QuantumCore;

public interface IChatManager
{
    void Talk(IEntity entity, string message);
    Task ShoutAsync(string message);
    Task NoticeAsync(string message, bool big = false);
}