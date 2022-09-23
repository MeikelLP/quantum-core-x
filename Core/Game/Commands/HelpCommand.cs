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
        public static async Task Help(IPlayerEntity player, ICommandManager commandManager, int page = 1)
        {
            var usableCmd = new Dictionary<string, CommandCache>();

            foreach (var cmd in commandManager.Commands)
            {
                if (commandManager.CanUseCommand((World.Entities.PlayerEntity) player, cmd.Key))
                    usableCmd.Add(cmd.Key, cmd.Value);
            }

            if (usableCmd.Count < 1)
            {
                await player.SendChatInfo("--- Help - Page 0/0 ---");
            }
            else
            {

                var allPages = (int) Math.Ceiling(usableCmd.Count / 5.0);

                if (page > allPages)
                    page = allPages;

                await player.SendChatInfo($"--- Help - Page {page}/{allPages} ---");

                var commandToShow = page * 5;

                if (commandToShow > usableCmd.Count)
                    commandToShow = usableCmd.Count;

                for (var i = (page - 1) * 5; i < commandToShow; i++)
                {
                    var command = usableCmd.ElementAt(i);
                    await player.SendChatInfo($"{command.Key}: {command.Value.Description}");
                }
            }
        }

        [CommandMethod("Shows an help with a specific command")]
        public static async Task HelpWithCommand(IPlayerEntity player, ICommandManager commandManager, string command)
        {
            if (!commandManager.Commands.ContainsKey(command) || !commandManager.CanUseCommand((World.Entities.PlayerEntity) player, command))
            {
                await player.SendChatInfo("Specified command does not exists");
            }
            else
            {
                var key = commandManager.Commands[command];

                await player.SendChatInfo($"--- Help for command {command} ---");

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

                    await player.SendChatInfo($"{command}{commandString}: {desc.Description}");
                }
            }
        }
    }
}
