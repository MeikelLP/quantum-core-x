using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using QuantumCore.Game.Packets;
using QuantumCore.API;
using QuantumCore.API.Game;
using Serilog;

namespace QuantumCore.Game.Commands
{
    public static class CommandManager
    {
        private static Dictionary<string, CommandCache> Commands = new Dictionary<string, CommandCache>();

        public static void Register(string ns, Assembly assembly = null)
        {
            Log.Debug($"Registring commands from namespace {ns}");
            if (assembly == null) assembly = Assembly.GetAssembly(typeof(CommandManager));

            var types = assembly.GetTypes().Where(t => string.Equals(t.Namespace, ns, StringComparison.Ordinal))
                .Where(t => t.GetCustomAttribute<CommandAttribute>() != null).ToArray();

            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<CommandAttribute>();
                Log.Debug($"Registring command {attr.Name} from {type.Name}");

                Commands[attr.Name] = new CommandCache(attr, type);
            }
        }

        public static void Handle(GameConnection connection, string chatline)
        {
            var args = chatline.Split(" ");
            string command = args[0].Substring(1);

            if (Commands.ContainsKey(command))
            {
                object[] objects = new object[args.Length];
                objects[0] = connection.Player.Player;

                for (int i = 1; i < args.Length; i++)
                {
                    float f;
                    int n;
                    string str = args[i];

                    if (str.Contains(".") || str.Contains(","))
                    {
                        str = str.Replace(".", ",");

                        if (float.TryParse(str, out f))
                            objects[i] = f;
                        else
                            objects[i] = str;
                    }
                    else
                    {
                        if (int.TryParse(str, out n))
                            objects[i] = n;
                        else
                            objects[i] = str;
                    }
                }
                
                Commands[command].Run(objects);
            }
            else
            {
                connection.Send(new ChatOutcoming()
                {
                    MessageType = ChatMessageTypes.Info,
                    Vid = 0,
                    Empire = 0,
                    Message = $"Unknown command {command}"
                });
            }
        }
    }
}
