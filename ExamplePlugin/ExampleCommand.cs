using QuantumCore.API;
using QuantumCore.API.Game;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using QuantumCore.API.Game.World;

namespace ExamplePlugin
{
    [Command("test", "This is an example command")]
    public static class TestCommand
    {
        [CommandMethod("Plain command without any parameter")]
        public static async Task Run(IPlayerEntity player)
        {
            await player.SendChatInfo("Test command works!");
        }

        [CommandMethod("This command tests the handler with more than one argument")]
        public static async Task Run(IPlayerEntity player, int required1, string optional1 = "")
        {
            await player.SendChatInfo($"Test command sent with parameters {required1} and {optional1}");
        }

        [CommandMethod("This command tests the float type")]
        public static async Task Run(IPlayerEntity player, float required1, string optional1 = "")
        {
            await player.SendChatInfo($"Test command sent with parameters {required1} and {optional1}");
        }
    }
}
