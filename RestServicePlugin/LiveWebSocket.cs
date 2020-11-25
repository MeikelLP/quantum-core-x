using System.Text;
using System.Threading.Tasks;
using EmbedIO.WebSockets;
using Newtonsoft.Json;
using QuantumCore.API.Game;
using Serilog;

namespace RestServicePlugin
{
    public class LiveWebSocket : WebSocketModule
    {
        private class RegisterListener
        {
            public string Type { get; set; }
            public string Map { get; set; }
        }

        private IGame _game;
        
        public LiveWebSocket(string urlPath, IGame game) : base(urlPath, true)
        {
            _game = game;
        }

        protected override async Task OnMessageReceivedAsync(IWebSocketContext context, byte[] buffer, IWebSocketReceiveResult result)
        {
            var register = JsonConvert.DeserializeObject<RegisterListener>(Encoding.UTF8.GetString(buffer));

            switch (register.Type)
            {
                case "mapUpdate":
                    break;
            }
        }
    }
}