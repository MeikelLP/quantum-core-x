using CommandLine;
using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands;

[Command("mall_password", "Opens the in-game item-shop warehouse")]
[CommandNoPermission]
public class MallPasswordCommand : ICommandHandler<MallPasswordCommandOptions>
{
    public Task ExecuteAsync(CommandContext<MallPasswordCommandOptions> context)
    {
        
        var password = context.Arguments.Password;
        if (string.IsNullOrEmpty(password))
        {
            context.Player.SendChatInfo("Please enter a password.");
            return Task.CompletedTask;
        }

        if (password.Length > 6)
        {
            context.Player.SendChatInfo("Password is too long.");
            return Task.CompletedTask;
        }
        
        // todo query the database for the password
        //
        // send this in case the password is wrong
        // pkPeer->EncodeHeader(HEADER_DG_SAFEBOX_WRONG_PASSWORD, dwHandle, 0);
        
        // todo load items from the safebox (cache/db)
        
        // send the packet with the safebox info
        // ClientManager.cpp:813 CClientManager::RESULT_SAFEBOX_LOAD(CPeer * pkPeer, SQLMsg * msg)
        
        return Task.CompletedTask;
    }
}

public class MallPasswordCommandOptions
{
    [Value(0)]
    public string Password { get; set; }
}
