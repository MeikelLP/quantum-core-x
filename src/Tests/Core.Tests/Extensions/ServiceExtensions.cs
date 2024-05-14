using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Xunit.Abstractions;

namespace Core.Tests.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddQuantumCoreTestLogger(this IServiceCollection services,
        ITestOutputHelper testOutputHelper)
    {
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddSerilog(new LoggerConfiguration()
                .WriteTo.TestOutput(testOutputHelper)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Query", LogEventLevel.Warning)
                .CreateLogger());
        });
        return services;
    }
}
