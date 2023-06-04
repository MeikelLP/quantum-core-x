using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.Core.Networking;
using QuantumCore.Game.PacketHandlers.Select;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers
{
    public class ChannelStatusHandler : ISelectPacketHandler<ChannelRequest>
    {
        private readonly ILogger<ChannelStatusHandler> _logger;
        public ChannelStatusHandler(ILogger<ChannelStatusHandler> logger) { 
            _logger = logger;
        }

        public async Task ExecuteAsync(PacketContext<ChannelRequest> ctx, CancellationToken token = default)
        {
            _logger.LogDebug("Channel request arrived!");

            var channel = new ChannelResponse{
                Status = (byte) 1
            };

            await ctx.Connection.Send(channel);
        }
    }
}
