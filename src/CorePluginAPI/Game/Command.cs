namespace QuantumCore.API.Game
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandAttribute : Attribute
    {
        public CommandAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
        
        public CommandAttribute(string name, string description, params string[] alias)
        {
            Name = name;
            Description = description;
            Alias = alias.ToList();
        }

        public string Name { get; }
        public string Description { get; }
        public List<string> Alias { get; set; } = [];
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
