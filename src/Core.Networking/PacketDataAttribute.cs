namespace QuantumCore.Networking;

/// <summary>
/// For internal use only
/// </summary>
[AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple = true)]
public class PacketDataAttribute : Attribute
{
    /// <summary>
    /// Static size without header or sub header.
    /// <remarks>
    ///     Due to attribute limitations of C# we cannot use <code>int?</code>.
    ///     Thus, this value will be interpreted as <code>null</code> if it is less than 0
    /// </remarks>
    /// </summary>
    public int StaticSize { get; set; }

    /// <summary>
    /// <see cref="StaticSize"/>
    /// </summary>
    /// <returns>Null if <see cref="StaticSize"/> is 0 or less</returns>
    public int? GetStaticSize()
    {
        return StaticSize <= 0
            ? null
            : StaticSize;
    }
}