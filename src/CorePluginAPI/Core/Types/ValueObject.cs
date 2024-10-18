namespace QuantumCore.API.Core.Types;

/// <summary>
/// Represents a value object that encapsulates a value of type <typeparamref name="TValue"/>.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public abstract class ValueObject<TValue> : IEquatable<ValueObject<TValue>> where TValue : IEquatable<TValue>
{
    /// <summary>
    /// Gets the encapsulated value.
    /// </summary>
    public TValue Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueObject{TValue}"/> class.
    /// </summary>
    /// <param name="value">The value to encapsulate.</param>
    /// <exception cref="ArgumentNullException">Thrown when the value is null.</exception>
    protected ValueObject(TValue value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        Value = value;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return Equals(obj as ValueObject<TValue>);
    }

    /// <summary>
    /// Determines whether the specified <see cref="ValueObject{TValue}"/> is equal to the current <see cref="ValueObject{TValue}"/>.
    /// </summary>
    /// <param name="other">The object to compare with the current object.</param>
    /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
    public bool Equals(ValueObject<TValue>? other)
    {
        return other! != null! && EqualityComparer<TValue>.Default.Equals(Value, other.Value);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return Value.ToString() ??
               throw new InvalidDataException("ValueObject.ToString() returned null"); //TODO: Correct approach?
    }

    /// <summary>
    /// Determines whether two specified <see cref="ValueObject{TValue}"/> instances are equal.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns>true if the two objects are equal; otherwise, false.</returns>
    public static bool operator ==(ValueObject<TValue> left, ValueObject<TValue> right)
    {
        return EqualityComparer<ValueObject<TValue>>.Default.Equals(left, right);
    }

    /// <summary>
    /// Determines whether two specified <see cref="ValueObject{TValue}"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first object to compare.</param>
    /// <param name="right">The second object to compare.</param>
    /// <returns>true if the two objects are not equal; otherwise, false.</returns>
    public static bool operator !=(ValueObject<TValue> left, ValueObject<TValue> right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Implicitly converts a <see cref="ValueObject{TValue}"/> to its encapsulated value.
    /// </summary>
    /// <param name="valueObject">The value object to convert.</param>
    public static implicit operator TValue(ValueObject<TValue> valueObject)
    {
        return valueObject.Value;
    }
}