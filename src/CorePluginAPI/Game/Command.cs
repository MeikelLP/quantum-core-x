namespace QuantumCore.API.Game
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CommandAttribute : Attribute
    {
        public CommandAttribute(string name, string description, string? shorthand = null)
        {
            Name = name;
            Description = description;
            Shorthand = shorthand;
        }

        public string Name { get; }
        public string Description { get; }
        public string? Shorthand { get; }
    }

    [AttributeUsage(AttributeTargets.Method)]
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
