using System;
using QuantumCore.API;
using QuantumCore.API.Game;

namespace ExamplePlugin
{
    public class Plugin : IPlugin
    {
        public string Name { get; } = "ExamplePlugin";
        public string Author { get; } = "QuantumCore Contributors";

        public void Register(object server)
        {
            Console.WriteLine("ExamplePlugin register!");

            if (server is IGame)
            {
                Console.WriteLine("Loading this plugin on a game server!");
                IGame game = (IGame)server;
                game.RegisterCommandNamespace(typeof(ExampleCommand));
            }
        }

        public void Unregister()
        {
            Console.WriteLine("ExamplePlugin unregister!");
        }
    }
}