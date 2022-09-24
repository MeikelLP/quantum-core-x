using System.Threading.Tasks;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game;

public interface IChatManager
{
    void Init();
    ValueTask Talk(IEntity entity, string message);
    Task Shout(string message);
}