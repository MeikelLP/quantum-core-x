using System.IO;
using QuantumCore.Core.Logging.Enrichers;
using Serilog;

namespace QuantumCore.Core.Logging
{
    public static class Configurator
    {
        private const string MessageTemplate = "[{Timestamp:HH:mm:ss}][{Level:u3}][{ProcessName:u5}|{MachineName}:" +
                                               "{EnvironmentUserName}]{Caller} >> {Message:lj} " +
                                               "{NewLine:1}{Exception:1}";

        /// <summary>
        /// Configure Serilog to use all necessary enrichers and the appropriate sinks.
        /// </summary>
        public static void EnableLogging()
        {
            LoggerConfiguration config = new LoggerConfiguration();

            // add minimum log level for the instances
#if DEBUG
            config.MinimumLevel.Verbose();
#else
            config.MinimumLevel.Information();
#endif

            // add destructuring for entities
            config.Destructure.ToMaximumDepth(4)
                .Destructure.ToMaximumCollectionCount(10)
                .Destructure.ToMaximumStringLength(100);

            // add environment variable
            config.Enrich.WithEnvironmentUserName()
                .Enrich.WithMachineName();

            // add process information
            config.Enrich.WithProcessId()
                .Enrich.WithProcessName();

            // add assembly information
            // TODO: uncomment if needed
            /* config.Enrich.WithAssemblyName() // {AssemblyName}
                .Enrich.WithAssemblyVersion(true) // {AssemblyVersion}
                .Enrich.WithAssemblyInformationalVersion(); */

            // add exception information
            config.Enrich.WithExceptionData();

            // add custom enricher for caller information
            config.Enrich.WithCaller();

            // sink to console
            config.WriteTo.Console(outputTemplate: MessageTemplate);

            // sink to rolling file
            config.WriteTo.RollingFile($"{Directory.GetCurrentDirectory()}/logs/api.log",
                fileSizeLimitBytes: 10 * 1024 * 1024,
                buffered: true,
                outputTemplate: MessageTemplate);

            // finally, create the logger
            Serilog.Log.Logger = config.CreateLogger();
        }
    }
}