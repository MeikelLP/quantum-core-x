public class SpecialItemGroup
{
    public class Drop
    {
        public uint ItemProtoId { get; init; }
        public uint Amount { get; init; }
        public float Chance { get; init; }
    }

    public uint SpecialItemId { get; init; }
    public List<Drop> Drops { get; init; } = [];

}