using System.ComponentModel;

namespace QuantumCore.API.Core;

/// <summary>
/// Hides standard Object members to make fluent interfaces easier to read.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IHideObjectMembers
{
    /// <summary>
    ///  Standard System.Object member.
    /// </summary>
    /// <returns>Standard result.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    Type GetType();

    /// <summary>
    /// Standard System.Object member.
    /// </summary>
    /// <returns>Standard result.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    int GetHashCode();

    /// <summary>
    /// Standard System.Object member.
    /// </summary>
    /// <returns>Standard result.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    string ToString();

    /// <summary>
    /// Standard System.Object member.
    /// </summary>
    /// <param name="other">The other.</param>
    /// <returns>Standard result.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    bool Equals(object other);
}