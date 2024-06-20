using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantumCore.Game.PacketHandlers.Game
{
    public class ItemUseToItemHandler : IGamePacketHandler<ItemUseToItem>
    {
        public Task ExecuteAsync(GamePacketContext<ItemUseToItem> ctx, CancellationToken token = default)
        {
            var player = ctx.Connection.Player;

            //todo: implement item use to item

            player.SendChatInfo("Item Use To Item");

            return Task.FromResult(1);
        }
    }
}
