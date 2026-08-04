using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Weikio.PluginFramework.Abstractions;

namespace Weikio.PluginFramework.Microsoft.DependencyInjection;

public class PluginProvider
{
    private readonly IEnumerable<IPluginCatalog> _catalogs;
    private readonly IServiceProvider _serviceProvider;

    public PluginProvider(IEnumerable<IPluginCatalog> catalogs, IServiceProvider serviceProvider)
    {
        _catalogs = catalogs;
        _serviceProvider = serviceProvider;
    }

    public ImmutableArray<Plugin> GetByTag(string tag)
    {
        return [.. _catalogs.SelectMany(x => x.GetByTag(tag))];
    }

    public ImmutableArray<Plugin> GetPlugins()
    {
        return [.. _catalogs.SelectMany(x => x.GetPlugins())];
    }

    public Plugin? Get(string name, Version version)
    {
        foreach (var pluginCatalog in _catalogs)
        {
            var result = pluginCatalog.Get(name, version);

            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    public ImmutableArray<T> GetTypes<T>() where T : class
    {
        var catalogs = _serviceProvider.GetServices<IPluginCatalog>();

        return
        [
            .. catalogs
                .Select(catalog => catalog.GetPlugins())
                .SelectMany(plugins => plugins.Where(x => typeof(T).IsAssignableFrom(x)),
                    (plugins, plugin) => plugin.Create<T>(_serviceProvider))
        ];
    }
}