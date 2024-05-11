using CommandLine;
using QuantumCore.API;
using QuantumCore.API.Game;
using QuantumCore.Extensions;
using QuantumCore.Game.Packets.General;
using QuantumCore.Game.Packets.Mall;

namespace QuantumCore.Game.Commands;

[Command("mall_password", "Opens the in-game item-shop warehouse")]
[CommandNoPermission]
public class MallPasswordCommand : ICommandHandler<MallPasswordCommandOptions>
{
    private readonly IItemManager _itemManager;
    
    public MallPasswordCommand(IItemManager itemManager)
    {
        _itemManager = itemManager;
    }
    
    public Task ExecuteAsync(CommandContext<MallPasswordCommandOptions> context)
    {
        var password = context.Arguments.Password;
        
        if (string.IsNullOrEmpty(password))
        {
            context.Player.SendChatInfo("Please enter a password.");
            return Task.CompletedTask;
        }

        if (password.Length != 6)
        {
            context.Player.SendChatInfo("Password is incorrect.");
            return Task.CompletedTask;
        }
        
        // todo grace period of 10 seconds if necessary
        // todo query the password and possibly cache it
        if (!string.Equals(password,"123456", StringComparison.InvariantCultureIgnoreCase))
        {
            context.Player.Connection.Send(new MallWrongPassword());
            return Task.CompletedTask;
        }
        
        // todo load items from the mall (cache/db)
        // safebox.cpp:55

        var proto = _itemManager.GetItem(11210)!;
        
        var test = new MallItem
        {
            Cell = new MallItemPosition
            {
                Type = 4,
                Cell = 1
            },
            Vid = 100,
            Count = 1,
            Flags = proto.Flags,
            AntiFlags = proto.AntiFlags,
            Highlight = 0,
            Sockets = new long[3],
            Bonuses = new ItemBonus[7]
        };
        
        context.Player.OpenMall();
        
        // context.Player.Connection.Send(test);
        
        return Task.CompletedTask;
    }
}

public class MallPasswordCommandOptions
{
    [Value(0)]
    public string Password { get; set; }
}
