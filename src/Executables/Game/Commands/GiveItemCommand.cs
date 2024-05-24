using CommandLine;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Caching;
using QuantumCore.Game.Extensions;

namespace QuantumCore.Game.Commands;

[Command("give", "Puts the given item in the inventory")]
public class GiveItemCommand : ICommandHandler<GiveCommandOptions>
{
    private readonly IItemManager _itemManager;
    private readonly ICacheManager _cacheManager;
    private readonly IWorld _world;

    public GiveItemCommand(IItemManager itemManager, ICacheManager cacheManager, IWorld world)
    {
        _itemManager = itemManager;
        _cacheManager = cacheManager;
        _world = world;
    }

    public async Task ExecuteAsync(CommandContext<GiveCommandOptions> context)
    {
        var target = string.Equals(context.Arguments.Target, "$self", StringComparison.InvariantCultureIgnoreCase)
            ? context.Player
            : _world.GetPlayer(context.Arguments.Target);

        if (target is null)
        {
            context.Player.SendChatInfo("Target not found");
        }
        else
        {
            var item = _itemManager.GetItem(context.Arguments.ItemId);
            if (item == null)
            {
                context.Player.SendChatInfo("Item not found");
                return;
            }

            var instance = new ItemInstance {Id = Guid.NewGuid(), ItemId = item.Id, Count = context.Arguments.Count};
            // Add item to players inventory
            if (!await target.Inventory.PlaceItem(instance))
            {
                // No space left in inventory, drop item with player name
                context.Player.SendChatInfo("No place in inventory");
                return;
            }

            // Store item in cache
            await instance.Persist(_cacheManager);

            // Send item to client
            target.SendItem(instance);
        }
    }
}

public class GiveCommandOptions
{
    [Value(0)] public string Target { get; set; } = "$self";

    [Value(1)] public uint ItemId { get; set; }

    [Value(2)] public byte Count { get; set; } = 1;
}