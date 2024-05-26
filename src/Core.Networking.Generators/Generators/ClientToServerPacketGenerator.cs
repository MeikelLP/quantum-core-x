using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace QuantumCore.Networking.Generators;

[Generator(LanguageNames.CSharp)]
public class ClientToServerPacketGenerator : IIncrementalGenerator
{
    private DeserializeGenerator _deserializeGenerator = null!;
    private GeneratorContext _generatorContext = null!;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyName = context.CompilationProvider.Select(static (c, _) => c);
        var sourceFiles = context.SyntaxProvider
            .ForAttributeWithMetadataName(GeneratorConstants.PACKET_CLIENT_TO_SERVER_ATTRIBUTE_FULLNAME,
                (node, token) =>
                {
                    return GeneratorConstants.IsReadonlyRefSpan(node, token) || GeneratorConstants.IsClass(node, token);
                }, GeneratorConstants.GetTypeInfo)
            .Collect()
            .SelectMany((info, _) => info.Distinct());

        var combined = sourceFiles.Combine(assemblyName);

        context.RegisterSourceOutput(combined, (spc, compilationPair) =>
        {
            var (info, compilation) = compilationPair;
            _generatorContext = new GeneratorContext();
            _deserializeGenerator = new DeserializeGenerator(_generatorContext);
            try
            {
                var name = info.Name;
                var ns = info.Namespace;
                var modifiers = info.Modifiers;

                var packetAttr = GeneratorConstants.GetClientToServerAttribute(info);
                // var clientToServerPacketInfo = GeneratorConstants.GetPacketInfo(packetAttr);
                var typeInfo = new PacketTypeInfo(name, ns, modifiers, info.Fields);
                var src = _deserializeGenerator.Generate(typeInfo);

                spc.AddSource($"{name}.ClientToServer.g.cs", SourceText.From(src, Encoding.UTF8));
            }
            catch (DiagnosticException e)
            {
                spc.ReportDiagnostic(e.Diagnostic);
            }
            catch (Exception e)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "QCX000001",
                        "Failed to generate packet serializer",
                        "Type {0} is setup incorrectly. Exception: {1} => {2}",
                        "generators",
                        DiagnosticSeverity.Error,
                        true), info.Location, info.Name,
                    e.GetType(), e.Message));
            }
        });
    }
}