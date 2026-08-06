using QuantumCore.API.Game;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands;

[Command("dance1", "Emote: Dance")]
public sealed class Dance1Command() : EmoteCommand("dance1");

[Command("dance2", "Emote: Dance")]
public sealed class Dance2Command() : EmoteCommand("dance2");

[Command("dance3", "Emote: Dance")]
public sealed class Dance3Command() : EmoteCommand("dance3");

[Command("dance4", "Emote: Dance")]
public sealed class Dance4Command() : EmoteCommand("dance4");

[Command("dance5", "Emote: Dance")]
public sealed class Dance5Command() : EmoteCommand("dance5");

[Command("dance6", "Emote: Dance")]
public sealed class Dance6Command() : EmoteCommand("dance6");

[Command("clap", "Emote: Clap")]
public sealed class ClapCommand() : EmoteCommand("clap");

[Command("cheer1", "Emote: Cheer1")]
public sealed class Cheer1Command() : EmoteCommand("cheer1");

[Command("cheer2", "Emote: Cheer2")]
public sealed class Cheer2Command() : EmoteCommand("cheer2");

[Command("congratulation", "Emote: Congratulation")]
public sealed class CongratulationCommand() : EmoteCommand("congratulation");

[Command("forgive", "Emote: Forgive")]
public sealed class ForgiveCommand() : EmoteCommand("forgive");

[Command("angry", "Emote: Angry")]
public sealed class AngryCommand() : EmoteCommand("angry");

[Command("attractive", "Emote: Attractive")]
public sealed class AttractiveCommand() : EmoteCommand("attractive");

[Command("sad", "Emote: Sad")]
public sealed class SadCommand() : EmoteCommand("sad");

[Command("shy", "Emote: Shy")]
public sealed class ShyCommand() : EmoteCommand("shy");

[Command("cheerup", "Emote: Cheerup")]
public sealed class CheerupCommand() : EmoteCommand("cheerup");

[Command("banter", "Emote: Banter")]
public sealed class BanterCommand() : EmoteCommand("banter");

[Command("joy", "Emote: Joy")]
public sealed class JoyCommand() : EmoteCommand("joy");

[Command("kiss", "Emote: Kiss")]
public sealed class KissCommand() : EmoteCommand("kiss", true);

[Command("french_kiss", "Emote: French Kiss")]
public sealed class FrenchKissCommand() : EmoteCommand("french_kiss", true);

[Command("slap", "Emote: Slap")]
public sealed class SlapCommand() : EmoteCommand("slap", true);

public abstract class EmoteCommand(string command, bool requiresTarget = false) : ICommandHandler
{
    public Task ExecuteAsync(CommandContext context)
    {
        var p = context.Player;
        if (requiresTarget && p.Target is null or not IPlayerEntity)
        {
            p.SendChatInfo("Emote requires a target");
        }
        else
        {
            p.SendChatCommand($"{command} {p.Vid} {p.Target?.Vid ?? 0}");
        }

        return Task.CompletedTask;
    }
}