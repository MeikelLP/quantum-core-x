namespace QuantumCore.Extensions;

public static class EnumExtensions
{
    public static bool TryParseEnum<TEnum>(string value, out TEnum result) where TEnum : struct, Enum
    {
        // Check if the value is a single word
        if (!value.Contains('_'))
        {
            // Capitalize the first letter and lowercase the rest to match enum naming convention
            var formattedSingleWord = char.ToUpper(value[0]) + value[1..].ToLower();
            return Enum.TryParse(formattedSingleWord, true, out result);
        }

        // If the value contains underscores, split and format it
        var parts = value.ToLower().Split('_');
        for (var i = 0; i < parts.Length; i++)
        {
            parts[i] = char.ToUpper(parts[i][0]) + parts[i][1..];
        }

        var formattedValue = string.Join("", parts);

        // Try to parse the formatted string as the enum type
        return Enum.TryParse(formattedValue, true, out result);
    }
}
