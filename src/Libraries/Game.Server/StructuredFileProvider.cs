using Microsoft.Extensions.FileProviders;
using QuantumCore.Game.Types;

namespace QuantumCore.Game;

public class StructuredFileProvider : IStructuredFileProvider
{
    private readonly IFileProvider _fileProvider;
    private static readonly char[] separator = [' ', '\t'];

    public StructuredFileProvider(IFileProvider fileProvider)
    {
        _fileProvider = fileProvider;
    }

    public async Task<StructuredFile> GetAsync(string path)
    {
        var file = _fileProvider.GetFileInfo(path);
        if (!file.Exists) throw new FileNotFoundException("Structured file not found", path);

        await using var fs = file.CreateReadStream();
        using var reader = new StreamReader(fs);
        var values = new Dictionary<string, string>();
        while (await reader.ReadLineAsync() is { } line)
        {
            line = line.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var i = line.IndexOfAny([' ', '\t']);
            if (i < 0) continue;

            var keyword = line.Substring(0, i);
            var startIndex = line.IndexOf(' ');
            if (startIndex == -1)
            {
                startIndex = line.IndexOf('\t');
            }

            if (startIndex == -1)
            {
                throw new InvalidOperationException(
                    "Line does not contain ' ' or '\t' char. One of those are required");
            }

            var value = line.Substring(startIndex).Split(separator).Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            values[keyword] = string.Join(' ', value);
        }

        return new StructuredFile(values);
    }
}