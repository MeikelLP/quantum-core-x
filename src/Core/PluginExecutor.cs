using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.API.PluginTypes;
using Weikio.PluginFramework.Abstractions;

namespace QuantumCore;

public class PluginExecutor
{
    private readonly ImmutableDictionary<Type, object[]> _allPlugins;

    public PluginExecutor(IEnumerable<IPluginCatalog> catalogs, IServiceProvider provider)
    {
        // all imported plugins (types)
        var allPlugins = catalogs
            .SelectMany(x => x.GetPlugins())
            .Select(x => x.Type)
            .ToArray();

        // all interfaces defined in the API assembly
        var pluginInterfaces = typeof(ISingletonPlugin).Assembly.GetExportedTypes().Where(x => x.IsInterface).ToArray();
        var dict = pluginInterfaces.ToDictionary(type => type, _ => new List<object>());

        foreach (var type in allPlugins)
        {
            // for each interface in a plugin type add an instance to the plugin dictionary
            // thus all instances are singletonsa
            var interfaces = type.GetInterfaces().Where(x => x.Assembly == typeof(ISingletonPlugin).Assembly);
            foreach (var intf in interfaces)
            {
                dict[intf].Add(ActivatorUtilities.CreateInstance(provider, type));
            }
        }
        _allPlugins = dict.ToImmutableDictionary(x => x.Key, x => x.Value.ToArray());
    }

    public async Task ExecutePlugins<T>(ILogger logger, Func<T, Task> action)
    {
        if (_allPlugins.TryGetValue(typeof(T), out var plugins) && plugins.Length > 0)
        {
            // prevent allocations as much as possible
            var taskArr = new Task[plugins.Length];
            for (var i = 0; i < plugins.Length; i++)
            {
                try
                {
                    taskArr[i] = action.Invoke((T) plugins[i]);
                }
                catch (Exception e)
                {
                    logger.LogError(e, "The plugin {PluginType} for {Interface} failed executing", plugins[i], typeof(T));
                    // exception is thrown before any await
                    taskArr[i] = Task.CompletedTask;
                }
            }

            try
            {
                await Task.WhenAll(taskArr).ConfigureAwait(false);
            }
            catch (AggregateException e)
            {
                logger.LogError(e, "Some plugins failed executing");
            }
        }
    }
}