using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.PluginFramework.Microsoft.DependencyInjection;

public class PluginFrameworkInitializer : IHostedService
{
    private readonly IEnumerable<IPluginCatalog> _pluginCatalogs;
    private readonly ILogger<PluginFrameworkInitializer> _logger;

    public PluginFrameworkInitializer(IEnumerable<IPluginCatalog> pluginCatalogs,
        ILogger<PluginFrameworkInitializer> logger)
    {
        _pluginCatalogs = pluginCatalogs;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Initializing {PluginCatalogCount} plugin catalogs", _pluginCatalogs.Count());

            foreach (var pluginCatalog in _pluginCatalogs)
            {
                try
                {
                    _logger.LogInformation("Initializing {PluginCatalog}", pluginCatalog);

                    await pluginCatalog.Initialize();

                    _logger.LogInformation("Initialized {PluginCatalog}", pluginCatalog.GetType().Name);
                    _logger.LogDebug("Found the following plugins from {PluginCatalog}:", pluginCatalog);

                    foreach (var plugin in pluginCatalog.GetPlugins())
                    {
                        _logger.LogTrace("Plugin loaded: {PluginName}", plugin.Name);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to initialize {PluginCatalog}", pluginCatalog);
                }
            }

            _logger.LogInformation("Initialized {PluginCatalogCount} plugin catalogs", _pluginCatalogs.Count());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to initialize plugin catalogs");

            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
