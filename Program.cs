using System;
using CommandLine;
using QuantumCore.Auth;
using QuantumCore.Core;
using QuantumCore.Game;

namespace QuantumCore
{
    class Program
    {
        public class Options 
        {
            [Option("auth", Required = false, HelpText = "Put the server into auth server mode")]
            public bool Auth { get; set; }
            [Option("game", Required = false, HelpText = "Put the server into game server mode")]
            public bool Game { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o => {
                IServer server = null;
                if(o.Auth) 
                {
                    server = new AuthServer();
                }
                else if(o.Game)
                {
                    server = new GameServer();
                }
                else 
                {
                    Console.WriteLine("Please specify the server mode");
                    System.Environment.Exit(1);
                }

                server.Start();
            });
        }
    }
}
