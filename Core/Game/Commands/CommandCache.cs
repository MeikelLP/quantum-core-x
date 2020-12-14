using System;
using System.Collections.Generic;
using System.Reflection;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;

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

        public List<CommandFunction> Functions = new List<CommandFunction>();

        public CommandCache(CommandAttribute attr, Type t)
        {
            Description = attr.Description;

            foreach (var method in t.GetMethods())
            {
                var spec = method.GetCustomAttribute<CommandMethodAttribute>();
                if (spec == null)
                {
                    continue;
                }

                var description = spec.Description;

                Functions.Add(new CommandFunction
                {
                    Description = description,
                    Method = method,
                });
            }
        }

        private bool IsTypeConvertable(Type input, Type expected)
        {
            if (input == expected)
            {
                return true;
            }
            
            if (input == typeof(int))
            {
                if (expected == typeof(uint) || expected == typeof(byte) || expected == typeof(ushort))
                {
                    return true;
                }
            }

            return false;
        }

        public void Run(object[] args)
        {
            MethodInfo method = null;
            
            foreach (var function in Functions)
            {
                var callArguments = new object[args.Length];
                Array.Copy(args, callArguments, args.Length);
                
                var param = function.Method.GetParameters();

                if (param.Length < args.Length)
                {
                    method = null;
                    continue;
                }
                
                method = function.Method;

                for (var i = 1; i < param.Length; i++) // Parameter 0 is always an IPlayer, no reason to check it
                {
                    if (param[i].HasDefaultValue && (callArguments.Length <= i))
                    {
                        Array.Resize(ref callArguments, callArguments.Length + 1);
                        callArguments[i] = Type.Missing;
                        continue;
                    }

                    if (callArguments.Length <= i)
                    {
                        method = null;
                        break;
                    }

                    if (param[i].ParameterType != callArguments[i].GetType())
                    {
                        if (IsTypeConvertable(callArguments[i].GetType(), param[i].ParameterType))
                        {
                            callArguments[i] = Convert.ChangeType(callArguments[i], param[i].ParameterType);
                        }
                        else if (param[i].ParameterType == typeof(IPlayerEntity))
                        {
                            if (callArguments[i].GetType() == typeof(string))
                            {
                                var player = World.World.Instance.GetPlayer((string) callArguments[i]);
                                if (player == null)
                                {
                                    ((IPlayerEntity) callArguments[0]).SendChatInfo($"Cannot find player {(string)callArguments[i]}");
                                    return;
                                }
                                callArguments[i] = player;
                            }
                            else
                            {
                                method = null;
                                break;
                            }
                        }
                        else
                        {
                            method = null;
                            break;
                        }
                    }
                }

                if (method != null)
                {
                    method.Invoke(null, callArguments);
                    return;
                }
            }

            ((IPlayerEntity)args[0]).SendChatInfo("Invalid command parameters");
        }
    }
}
