using System.Globalization;

namespace QuantumCore.Core.Types;

public class StructuredFile
{
    private readonly Dictionary<string, string> _values;

    public StructuredFile(Dictionary<string, string> values)
    {
        _values = values;
    }

    /// <summary>
    /// Returns the value of the given key
    /// </summary>
    /// <param name="key">The key to look for</param>
    /// <returns>The normalized value or null if the key wasn't found</returns>
    public string? GetValue(string key)
    {
        return !_values.ContainsKey(key) ? null : _values[key];
    }

    /// <summary>
    /// Returns the value of the given key
    /// </summary>
    /// <param name="key">The key to look for</param>
    /// <returns>The normalized value or null if the key wasn't found or the value was invalid</returns>
    public float? GetFloatValue(string key)
    {
        var value = GetValue(key);
        if (value == null) return null;

        if (!float.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out var f)) return null;
        return f;
    }

    /// <summary>
    /// Returns the positioned float value for the given key
    /// </summary>
    /// <param name="key">The key to look for</param>
    /// <param name="position">The relevant position</param>
    /// <returns>The float value or null if the key was not found, or the position was invalid, or the value was invalid</returns>
    public float? GetFloatValue(string key, int position)
    {
        var value = GetValue(key);
        if (value == null) return null;

        var values = value.Split(' ');
        if (position >= values.Length) return null;

        if (!float.TryParse(values[position], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out var f))
            return null;

        return f;
    }
}