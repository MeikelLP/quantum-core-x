namespace QuantumCore.API.Game
{
    public record PermissionGroup
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = "";
        public IList<uint> Users { get; init; } = new List<uint>();
        public IList<string> Permissions { get; init; } = new List<string>();
    }
}