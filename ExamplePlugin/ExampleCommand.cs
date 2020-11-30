using QuantumCore.API;
using QuantumCore.API.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExamplePlugin
{
    public class ExampleCommand : ICommand
    {
        public void Execute(IConnection connection, string[] args) => Console.WriteLine("test command works!");

        public string GetDescription()
        {
            return "";
        }

        public string GetName()
        {
            return "test";
        }
    }

    public class ExampleCommand2 : ICommand
    {
        public void Execute(IConnection connection, string[] args) => Console.WriteLine("test command works again!");

        public string GetDescription()
        {
            return "aaaaaaaaaaaaaaaa";
        }

        public string GetName()
        {
            return "test2";
        }
    }

    public class ExampleCommandOverride : ICommand
    {
        public void Execute(IConnection connection, string[] args) => Console.WriteLine("test command is overwritten!");

        public string GetDescription()
        {
            return "xd";
        }

        public string GetName()
        {
            return "test";
        }

    }
}
