using CommandLine;
using QuantumCore.API;
using QuantumCore.API.Game;
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
        
        if (context.Player.Mall.LastInteraction.HasValue)
        {
            var time = DateTime.UtcNow - context.Player.Mall.LastInteraction.Value;
            var secondsRemaining = (int) (10 - time.TotalSeconds);
            if (secondsRemaining > 0)
            {
                context.Player.SendChatInfo($"Please wait {secondsRemaining} seconds before trying again.");
                return Task.CompletedTask;
            }
        }
        
        // todo query the password and possibly cache it
        if (!string.Equals(password,"123456", StringComparison.InvariantCultureIgnoreCase))
        {
            context.Player.Connection.Send(new MallWrongPassword());
            return Task.CompletedTask;
        }
        
        context.Player.Mall.Open();
        context.Player.Mall.SendItems();
        
        return Task.CompletedTask;
    }
}

public class MallPasswordCommandOptions
{
    [Value(0)]
    public string Password { get; set; }
}
