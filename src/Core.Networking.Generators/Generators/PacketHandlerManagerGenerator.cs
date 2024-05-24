// using System.Text;
// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.Text;
//
// namespace QuantumCore.Networking.Generators;
//
// [Generator(LanguageNames.CSharp)]
// public class PacketHandlerManagerGenerator : IIncrementalGenerator
// {
//     public void Initialize(IncrementalGeneratorInitializationContext context)
//     {
//         var sourceFiles = context.SyntaxProvider
//             .ForAttributeWithMetadataName(GeneratorConstants.PACKET_CLIENT_TO_SERVER_ATTRIBUTE_FULLNAME, GeneratorConstants.IsReadonlyRefSpan, GeneratorConstants.GetTypeInfo)
//             .Collect()
//             .SelectMany((info, _) => info.Distinct());
//
//         var combined = sourceFiles.Collect().Combine(context.CompilationProvider);
//
//         context.RegisterSourceOutput(combined, (spc, input) =>
//         {
//             try
//             {
//                 var (typeInfos, compilation) = input;
//                 var attributeType = compilation.GetTypeByMetadataName(GeneratorConstants.PACKET_SERVER_TO_CLIENT_ATTRIBUTE_FULLNAME)!;
//                 var source = GeneratorConstants.GeneratePackageHandler(input.Right.AssemblyName, typeInfos, attributeType);
//
//                 spc.AddSource($"{input.Right.AssemblyName}PacketHandlerManager.g.cs", SourceText.From(source, Encoding.UTF8));
//             }
//             catch (DiagnosticException e)
//             {
//                 spc.ReportDiagnostic(e.Diagnostic);
//             }
//             catch (Exception e)
//             {
//                 spc.ReportDiagnostic(Diagnostic.Create(
//                     new DiagnosticDescriptor(
//                         "QCX000008",
//                         "Failed to generate PacketHandlerManager",
//                         "Unknown error. Exception: {1} => {2}",
//                         "generators",
//                         DiagnosticSeverity.Error,
//                         true), null, e.GetType(), e.Message));
//             }
//         });
//     }
// }

