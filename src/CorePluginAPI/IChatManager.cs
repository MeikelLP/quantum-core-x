using System.Threading.Tasks;
using QuantumCore.API.Game.World;

namespace QuantumCore.API;

public interface IChatManager
{
    void Init();
    void Talk(IEntity entity, string message);
    Task Shout(string message);
}
