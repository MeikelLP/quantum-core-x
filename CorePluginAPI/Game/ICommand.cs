using System;

namespace QuantumCore.API.Game
{
    /// <summary>
    /// This interface rapresents an in-game command usable in the chat.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// The name of the command.
        /// 
        /// If the command is "/hello" the name of the command
        /// is "hello"
        /// </summary>
        /// <returns>A string that rapresents the name</returns>
        public string GetName();

        /// <summary>
        /// The description of the command used by the "help" command and
        /// probably by any future command.
        /// </summary>
        /// <returns>A string that rapresents the description</returns>
        public string GetDescription();

        /// <summary>
        /// This method will be invoked when a command is executed.
        /// </summary>
        /// <param name="connection">The connection that invoked the command.</param>
        /// <param name="args">A list containing all arguments of the string.
        ///     The first parameters IS ALWAYS the command name.</param>
        public void Execute(IConnection connection, string[] args);
    }
}
