using QuantumCore.API;
using QuantumCore.API.Game;
using System;
using System.Collections.Generic;
using System.Text;
using QuantumCore.API.Game.World;

namespace ExamplePlugin
{
    [Command("test", "This is an example command")]
    public static class TestCommand
    {
        [CommandMethod("Plain command without any parameter")]
        public static void Run(IPlayerEntity player)
        {
            player.SendChatInfo("Test command works!");
        }

        [CommandMethod("This command tests the handler with more than one argument")]
        public static void Run(IPlayerEntity player, int required1, string optional1 = "")
        {
            player.SendChatInfo($"Test command sent with parameters {required1} and {optional1}");
        }

        [CommandMethod("This command tests the float type")]
        public static void Run(IPlayerEntity player, float required1, string optional1 = "")
        {
            player.SendChatInfo($"Test command sent with parameters {required1} and {optional1}");
        }
    }
}
