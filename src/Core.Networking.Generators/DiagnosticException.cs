using Microsoft.CodeAnalysis;

namespace QuantumCore.Networking;

public class DiagnosticException : Exception
{
    public Diagnostic Diagnostic { get; }

    public DiagnosticException(string code, string message, Location location, params object[] args)
    {
        Diagnostic = Diagnostic.Create(new DiagnosticDescriptor(
            code,
            "Failed to generate packet serializer",
            message,
            "generators",
            DiagnosticSeverity.Error,
            true), location, args);
    }

}