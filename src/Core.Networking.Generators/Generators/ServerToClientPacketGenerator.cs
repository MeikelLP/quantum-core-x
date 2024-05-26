using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace QuantumCore.Networking.Generators;

[Generator(LanguageNames.CSharp)]
public class ServerToClientPacketGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var compilation = context.CompilationProvider.Select(static (c, _) => c);
        var sourceFiles = context.SyntaxProvider
            .ForAttributeWithMetadataName(GeneratorConstants.PACKET_SERVER_TO_CLIENT_ATTRIBUTE_FULLNAME,
                delegate(SyntaxNode node, CancellationToken token)
                {
                    return GeneratorConstants.IsReadonlyRefSpan(node, token) || GeneratorConstants.IsClass(node, token);
                }, GeneratorConstants.GetTypeInfo)
            .Collect()
            .SelectMany((info, _) => info.Distinct());

        var combined = sourceFiles.Combine(compilation);

        context.RegisterSourceOutput(combined, (spc, compilationPair) =>
        {
            var (info, compilation) = compilationPair;
            try
            {
                var name = info.Name;
                var ns = info.Namespace;
                var modifiers = info.Modifiers;

                var attributeType =
                    compilation.GetTypeByMetadataName(GeneratorConstants.PACKET_SERVER_TO_CLIENT_ATTRIBUTE_FULLNAME)!;
                var serverToClientPacketInfo = GeneratorConstants.GetPacketInfo(info, attributeType);
                var packetInfo = new PacketTypeInfo(name, ns, modifiers, info.Fields);

                var src = new SerializeGenerator().Generate(packetInfo, serverToClientPacketInfo);

                spc.AddSource($"{name}.ServerToClient.g.cs", SourceText.From(src, Encoding.UTF8));
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
                        true), info.Location, info.FullName, e.GetType(), e.Message));
            }
        });
    }
}