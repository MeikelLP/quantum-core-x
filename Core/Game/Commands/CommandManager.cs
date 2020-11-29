using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using QuantumCore.Core.Networking;
using QuantumCore.Game.Packets;
using Serilog;

namespace QuantumCore.Game.Commands
{
    public class TestCommand : ICommand
    {
        public void Execute(IConnection connection, string[] args) => connection.Send(new ChatOutcoming()
        {
            MessageType = ChatMessageTypes.Info,
            Vid = 0,
            Empire = 0,
            Message = "This is a test command!"
        });

        public string GetDescription() => "This is an example command";

        public string GetName() => "test";
    }

    public static class CommandManager
    {
        private static Dictionary<string, ICommand> Commands = new Dictionary<string, ICommand>();

        public static void Register(string ns, Assembly assembly = null)
        {
            Log.Debug($"Registring commands from namespace {ns}");
            if (assembly == null) assembly = Assembly.GetAssembly(typeof(CommandManager));

            var types = assembly.GetTypes().Where(t => string.Equals(t.Namespace, ns, StringComparison.Ordinal))
                .Where(t => t.GetInterface("ICommand") != null).ToArray();

            foreach (var type in types)
            {
                var command = (ICommand)Activator.CreateInstance(type);

                Log.Debug($"Registring command {command.GetName()}");

                Commands.Add(command.GetName(), command);
            }
        }

        public static void Handle(IConnection connection, string chatline)
        {
            var args = chatline.Split(" ");
            string command = args[0].Substring(1);

            if (Commands.ContainsKey(command))
            {
                Commands[command].Execute(connection, args);
            }
            else
            {
                connection.Send(new ChatOutcoming()
                {
                    MessageType = ChatMessageTypes.Info,
                    Vid = 0,
                    Empire = 0,
                    Message = $"Unknown command ${command}"
                });
            }
        }
    }
}
