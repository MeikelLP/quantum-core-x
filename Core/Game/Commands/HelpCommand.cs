using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands
{
    [Command("help", "Shows this help message")]
    [CommandNoPermission]
    public static class HelpCommand
    {
        [CommandMethod]
        public static Task Help(IPlayerEntity player, int page = 1)
        {
            var usableCmd = new Dictionary<string, CommandCache>();

            foreach (var cmd in CommandManager.Commands)
            {
                if (CommandManager.CanUseCommand((World.Entities.PlayerEntity) player, cmd.Key))
                    usableCmd.Add(cmd.Key, cmd.Value);
            }

            if (usableCmd.Count < 1)
            {
                player.SendChatInfo("--- Help - Page 0/0 ---");
            }
            else
            {

                var allPages = (int) Math.Ceiling(usableCmd.Count / 5.0);

                if (page > allPages)
                    page = allPages;

                player.SendChatInfo($"--- Help - Page {page}/{allPages} ---");

                var commandToShow = page * 5;

                if (commandToShow > usableCmd.Count)
                    commandToShow = usableCmd.Count;

                for (var i = (page - 1) * 5; i < commandToShow; i++)
                {
                    var command = usableCmd.ElementAt(i);
                    player.SendChatInfo($"{command.Key}: {command.Value.Description}");
                }
            }
            
            return Task.CompletedTask;
        }

        [CommandMethod("Shows an help with a specific command")]
        public static Task HelpWithCommand(IPlayerEntity player, string command)
        {
            if (!CommandManager.Commands.ContainsKey(command) || !CommandManager.CanUseCommand((World.Entities.PlayerEntity) player, command))
            {
                player.SendChatInfo("Specified command does not exists");
            }
            else
            {
                var key = CommandManager.Commands[command];

                player.SendChatInfo($"--- Help for command {command} ---");

                string commandString;

                foreach (var desc in key.Functions)
                {
                    commandString = "";

                    foreach (var param in desc.Method.GetParameters())
                    {
                        if (param.Position == 0)
                            continue;

                        if (param.HasDefaultValue)
                            commandString += " [";
                        else
                            commandString += " <";

                        commandString += $"{param.ParameterType.Name}:{param.Name}";

                        if (param.HasDefaultValue)
                            commandString += "]";
                        else
                            commandString += ">";
                    }

                    player.SendChatInfo($"{command}{commandString}: {desc.Description}");
                }
            }

            return Task.CompletedTask;
        }
    }
}
