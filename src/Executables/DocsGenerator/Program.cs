using DocsGenerator;

var currentDirectory = Directory.GetCurrentDirectory();
if (currentDirectory.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar))
{
    // get out of the bin directory in development
    currentDirectory = Path.GetFullPath(Path.Combine(currentDirectory, "..", "..", ".."));
}

var targetDir = Path.Combine(currentDirectory, "..", "..", "..", "docs", "docs");

await Task.WhenAll(
    PacketDocsGenerator.GenerateAsync(targetDir),
    CommandDocsGenerator.GenerateAsync(targetDir)
);
