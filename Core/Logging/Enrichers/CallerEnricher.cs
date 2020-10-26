using System.Diagnostics;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace QuantumCore.Core.Logging.Enrichers
{
    /// <summary>
    /// https://gist.github.com/nblumhardt/0e1e22f50fe79de60ad257f77653c813, but customized
    /// </summary>
    public class CallerEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var skip = 3;
            while (true)
            {
                var stack = new StackFrame(skip);
                if (!stack.HasMethod())
                {
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue("<unknown method>")));
                    return;
                }

                var method = stack.GetMethod();
                if (method.DeclaringType?.Assembly != typeof(Log).Assembly)
                {
                    var caller = $"{method.DeclaringType?.Name}.{method.Name}";
                    logEvent.AddPropertyIfAbsent(new LogEventProperty("Caller", new ScalarValue(caller)));
                    return;
                }

                skip++;
            }
        }
    }

    public static class LoggerCallerEnrichmentConfiguration
    {
        public static LoggerConfiguration WithCaller(this LoggerEnrichmentConfiguration enrichmentConfiguration)
        {
            return enrichmentConfiguration.With<CallerEnricher>();
        }
    }
}