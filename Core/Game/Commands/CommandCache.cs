using System;
using System.Collections.Generic;
using System.Reflection;
using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands
{
    
    public class CommandFunction
    {
        public string Description { get; set; }

        public MethodInfo Method { get; set; }
    }

    public class CommandCache
    {
        public string Description { get; protected set; }

        private Type Type;

        private List<CommandFunction> Functions = new List<CommandFunction>();

        public CommandCache(CommandAttribute attr, Type t)
        {
            Type = t;
            Description = attr.Description;

            foreach (var method in t.GetMethods())
            {
                if (method.Name == "Run")
                {
                    var spec = method.GetCustomAttribute<CommandMethodAttribute>();
                    string description = "";

                    if (spec != null)
                    {
                        description = spec.Description;
                    }

                    var func = new CommandFunction
                    {
                        Description = description,
                        Method = method,
                    };

                    Functions.Add(func);
                }
            }
        }

        public void Run(object[] args)
        {
            bool isUsable = true;
            MethodInfo method = null;

            foreach (var function in Functions)
            {
                var param = function.Method.GetParameters();

                if (param.Length < args.Length)
                {
                    isUsable = false;
                    continue;
                }

                isUsable = true;
                method = function.Method;

                for (int i = 1; i < param.Length; i++) // Parameter 0 is always an IPlayer, no reason to check it
                {
                    if (param[i].HasDefaultValue && (args.Length <= i))
                    {
                        Array.Resize(ref args, args.Length + 1);
                        args[i] = Type.Missing;
                        continue;
                    }

                    if (args.Length <= i)
                    {
                        isUsable = false;
                        break;
                    }

                    if (param[i].ParameterType != args[i].GetType())
                    {
                        isUsable = false;
                        break;
                    }
                }

                if (isUsable)
                    break;
            }

            if (isUsable)
            {
                object obj = Activator.CreateInstance(Type);
                method.Invoke(obj, args);
            }
            
            //else
                // TODO: Should expose something like args[0].SendChatMessage(ChatType.Info, "Invalid parameters .....");

        }
    }
}
