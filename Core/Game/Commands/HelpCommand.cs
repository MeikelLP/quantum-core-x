using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands
{
    [Command("help", "Shows this help message")]
    public static class HelpCommand
    {
        [CommandMethod]
        public static async void Help(IPlayerEntity player, int page = 1)
        {
            if (CommandManager.Commands.Count < 1)
            {
                player.SendChatInfo("--- Help - Page 0/0 ---");
            }
            else
            {

                var allPages = CommandManager.Commands.Count / 5;
                allPages += 1;

                if (page > allPages)
                    page = allPages;

                player.SendChatInfo($"--- Help - Page {page}/{allPages} ---");

                var commandToShow = page * 5;

                if (commandToShow > CommandManager.Commands.Count)
                    commandToShow = CommandManager.Commands.Count;

                for (var i = (page - 1) * 5; i < commandToShow; i++)
                {
                    var command = CommandManager.Commands.ElementAt(i);

                    player.SendChatInfo($"{command.Key}: {command.Value.Description}");
                }
            }
        }

        [CommandMethod("Shows an help with a specific command")]
        public static async void HelpWithCommand(IPlayerEntity player, string command)
        {
            if (!CommandManager.Commands.ContainsKey(command))
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

        }
    }
}
