using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace QuantumCore.Core.Types
{
    public class StructuredFile
    {
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>();

        /// <summary>
        /// Parses the given file
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <exception cref="FileNotFoundException">Thrown if the given file wasn't found</exception>
        public async Task ReadAsync(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Structured file not found", path);
            
            using var reader = new StreamReader(path);
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                line = line.Trim();
                if(string.IsNullOrWhiteSpace(line)) continue;

                var i = line.IndexOfAny(new[] {' ', '\t'});
                if(i < 0) continue;
                
                var keyword = line.Substring(0, i);
                var value = line.Substring(line.IndexOf(' ')).Split(new []{' ', '\t'}).Select(s => s.Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                _values[keyword] = string.Join(' ', value);
            }
        }

        /// <summary>
        /// Returns the value of the given key
        /// </summary>
        /// <param name="key">The key to look for</param>
        /// <returns>The normalized value or null if the key wasn't found</returns>
        [CanBeNull]
        public string GetValue(string key)
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

            if (!float.TryParse(values[position], NumberStyles.Float, NumberFormatInfo.InvariantInfo, out var f)) return null;
            
            return f;
        }
    }
}