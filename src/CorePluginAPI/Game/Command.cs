using System;
using JetBrains.Annotations;

namespace QuantumCore.API.Game
{
    [AttributeUsage(AttributeTargets.Class)]
    [MeansImplicitUse]
    public class CommandAttribute : Attribute
    {
        public CommandAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; }
        public string Description { get; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse]
    public class CommandMethodAttribute : Attribute
    {
        public CommandMethodAttribute(string description = "")
        {
            Description = description;
        }

        public string Description { get; }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CommandNoPermissionAttribute : Attribute {}
}
