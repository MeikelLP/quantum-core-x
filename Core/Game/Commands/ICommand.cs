using System;
using System.Collections.Generic;
using System.Text;
using QuantumCore.Core.Networking;

namespace QuantumCore.Game.Commands
{
    interface ICommand
    {
        public string GetName();

        public string GetDescription();

        public void Execute(IConnection connection, string[] args);
    }
}
