using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using QuantumCore.Extensions;

namespace QuantumCore;

public static class QuantumCoreHost
{
    public static HostApplicationBuilder CreateBuilder(string[] args)
    {
        // workaround for https://github.com/dotnet/project-system/issues/3619
        var assemblyPath = Assembly.GetEntryAssembly()?.Location;
        if (!string.IsNullOrWhiteSpace(assemblyPath))
        {
            // may be null in single file deployment
            Directory.SetCurrentDirectory(Path.GetDirectoryName(assemblyPath)!);
        }

        var preferredDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".."));
        if (!Directory.GetFiles(preferredDir).Any(x => x.EndsWith(".csproj")))
        {
            preferredDir = Directory.GetCurrentDirectory();
        }

        var contentRoot = Path.Combine(preferredDir, "data");
        var host = new HostApplicationBuilder(args) { Environment = { ContentRootPath = contentRoot } };
        if (!host.Environment.IsDevelopment())
        {
            host.Services.Configure<ConsoleLifetimeOptions>(opts => opts.SuppressStatusMessages = true);
        }

        host.Services.AddSingleton<IFileProvider>(provider =>
        {
            var contentRootPath = provider.GetRequiredService<IHostEnvironment>().ContentRootPath;
            if (!Directory.Exists(contentRootPath)) Directory.CreateDirectory(contentRootPath);
            return new PhysicalFileProvider(contentRootPath);
        });
        host.Services.AddCoreServices(host.Configuration);

        return host;
    }
}