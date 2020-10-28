using System;
using QuantumCore.API;

namespace ExamplePlugin
{
    public class Plugin : IPlugin
    {
        public string Name { get; } = "ExamplePlugin";
        public string Author { get; } = "QuantumCore Contributors";
        
        public void Register()
        {
            Console.WriteLine("ExamplePlugin register!");
        }
    }
}