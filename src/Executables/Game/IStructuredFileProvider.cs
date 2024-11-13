using QuantumCore.Core.Types;

namespace QuantumCore.Game;

public interface IStructuredFileProvider
{
    Task<StructuredFile> GetAsync(string path);
}