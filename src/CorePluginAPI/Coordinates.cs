namespace QuantumCore.API;

public record struct Coordinates(uint X, uint Y)
{
    override public string ToString() => $"({X}, {Y})";
}
