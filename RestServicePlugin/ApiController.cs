using System.Linq;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;

namespace RestServicePlugin
{
    public class ApiController : WebApiController
    {
        private IGame _game;
        
        public ApiController(IGame game)
        {
            _game = game;
        }

        private object FormatEntity(IEntity entity)
        {
            if (entity is IPlayerEntity player)
            {
                return new
                {
                    Type = "player",
                    Vid = entity.Vid,
                    PositionX = entity.PositionX,
                    PositionY = entity.PositionY,
                    Rotation = entity.Rotation,
                    Name = player.Name
                };
            }
            
            return new
            {
                Type = "entity",
                Vid = entity.Vid,
                PositionX = entity.PositionX,
                PositionY = entity.PositionY,
                Rotation = entity.Rotation
            };
        }
        
        [Route(HttpVerbs.Get, "/entities/{mapName}")]
        public async Task<object> Test(string mapName)
        {
            var map = _game.World.GetMapByName(mapName);
            var entities = map.GetEntities();

            return new
            {
                Name = map.Name,
                PositionX = map.PositionX,
                PositionY = map.PositionY,
                Width = map.Width,
                Height = map.Height,
                Entities = entities.Select(FormatEntity)
            };
        }
    }
}