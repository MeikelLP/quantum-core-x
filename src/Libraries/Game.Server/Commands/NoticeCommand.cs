using CommandLine;
using QuantumCore.API;
using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands;

[Command("notice", "Broadcast a notice to all online players")]
[Command("b", "Broadcast a notice to all online players")]
public sealed class NoticeCommand : ICommandHandler<NoticeOptions>
{
    private readonly IChatManager _chatManager;

    public NoticeCommand(IChatManager chatManager)
    {
        _chatManager = chatManager;
    }

    public async Task ExecuteAsync(CommandContext<NoticeOptions> ctx)
    {
        if (string.IsNullOrWhiteSpace(ctx.Arguments.Message))
        {
            ctx.Player.SendChatInfo("Usage: /notice <message>");
            return;
        }

        await _chatManager.NoticeAsync(ctx.Arguments.Message, ctx.Arguments.Big);
    }
}

public sealed class NoticeOptions
{
    [Value(0, MetaName = "message", Required = true)]
    public string Message { get; set; } = "";

    [Option('b', "big", HelpText = "Show as big centred notice")]
    public bool Big { get; set; }
}