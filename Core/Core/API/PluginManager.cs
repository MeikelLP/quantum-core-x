using System;
using System.IO;
using System.Linq;
using System.Reflection;
using McMaster.NETCore.Plugins;
using QuantumCore.API;
using Serilog;

namespace QuantumCore.Core.API
{
    public static class PluginManager
    {
        public static string[] PluginPaths { get; } = new[]
        {
            "Plugins/"
        };

        public static void LoadPlugins(object server)
        {
            Log.Information("Load plugins");
            var plugins = PluginPaths.Where(Directory.Exists).SelectMany(Directory.GetDirectories).ToList();
            foreach (var pluginPath in plugins)
            {
                var pluginName = Path.GetFileName(pluginPath);
                var pluginDll = Path.Combine(pluginPath, pluginName + ".dll");

                if (File.Exists(pluginDll))
                {
                    IPlugin plugin = null;
                    
                    var loader = PluginLoader.CreateFromAssemblyFile(
                        Path.GetFullPath(pluginDll),
                        sharedTypes: new[] {typeof(IPlugin), typeof(Log)},
                        configure: (config) =>
                        {
                            config.EnableHotReload = true;
                        }
                    );
                    loader.Reloaded += (sender, args) =>
                    {
                        Log.Debug($"Reload plugin {pluginPath}");
                        plugin?.Unregister();
                        plugin = RegisterPlugin(loader, server);
                    };
                    
                    Log.Debug($"Load Plugin {pluginPath}");
                    plugin = RegisterPlugin(loader, server);
                }
                else
                {
                    Log.Warning($"Missing dll in plugin {pluginPath}");
                }
            }
        }

        private static IPlugin RegisterPlugin(PluginLoader loader, object server)
        {
            var pluginAssembly = loader.LoadDefaultAssembly();
                    
            // Search entry point
            foreach (var type in pluginAssembly.GetTypes())
            {
                if (typeof(IPlugin).IsAssignableFrom(type))
                {
                    var plugin = Activator.CreateInstance(type) as IPlugin;
                    if (plugin != null)
                    {
                        plugin.Register(server);
                        Log.Information($"Plugin {plugin.Name} by {plugin.Author} loaded");
                    }

                    return plugin;
                }
            }

            return null;
        }
    }
}