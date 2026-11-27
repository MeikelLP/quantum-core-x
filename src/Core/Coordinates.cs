using System.Numerics;

namespace QuantumCore.API;

public record struct Coordinates(uint X, uint Y)
{
    public override string ToString() => $"({X}, {Y})";

    public static Coordinates operator *(Coordinates a, uint multiplier) => new(a.X * multiplier, a.Y * multiplier);
    public static Coordinates operator +(Coordinates a, Coordinates b) => new(a.X + b.X, a.Y + b.Y);

    public static Coordinates operator +(Coordinates a, Vector2 delta)
    {
        return new Coordinates(checked((uint)(a.X + delta.X)), checked((uint)(a.Y + delta.Y)));
    }
    
    // throw on underflow with checked()
    public static Coordinates operator -(Coordinates a, Coordinates b) => new(checked(a.X - b.X), checked(a.Y - b.Y));
}
