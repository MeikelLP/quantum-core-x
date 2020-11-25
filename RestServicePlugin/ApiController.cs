using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using QuantumCore.API.Game;

namespace RestServicePlugin
{
    public class ApiController : WebApiController
    {
        private IGame _game;
        
        public ApiController(IGame game)
        {
            _game = game;
        }
        
        [Route(HttpVerbs.Get, "/test")]
        public async Task<object> Test()
        {
            var map = _game.World.GetMapByName("metin2_map_a1");
            
            return new
            {
                test = map.ToString()
            };
        }
    }
}