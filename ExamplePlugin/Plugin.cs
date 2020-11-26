using System;
using QuantumCore.API;

namespace ExamplePlugin
{
    public class Plugin : IPlugin
    {
        public string Name { get; } = "ExamplePlugin";
        public string Author { get; } = "QuantumCore Contributors";

        public void Register(object server)
        {
            Console.WriteLine("ExamplePlugin register!");
        }

        public void Unregister()
        {
            Console.WriteLine("ExamplePlugin unregister!");
        }
    }
}