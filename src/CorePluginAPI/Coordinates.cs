namespace QuantumCore.API;

public record struct Coordinates(uint X, uint Y)
{
    public override string ToString() => $"({X}, {Y})";

    public static Coordinates operator *(Coordinates a, uint multiplier) => new(a.X * multiplier, a.Y * multiplier);
    public static Coordinates operator +(Coordinates a, Coordinates b) => new(a.X + b.X, a.Y + b.Y);
}
