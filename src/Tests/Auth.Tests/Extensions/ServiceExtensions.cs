using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Auth.Tests.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddQuantumCoreTestLogger(this IServiceCollection services)
    {
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddSerilog(new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Query", LogEventLevel.Warning)
                .CreateLogger());
        });
        return services;
    }
}