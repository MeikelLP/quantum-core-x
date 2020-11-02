using System;
using System.IO;
using System.Linq;
using System.Reflection;
using QuantumCore.API;
using Serilog;

namespace QuantumCore.Core.API
{
    public class PluginManager
    {
        public static string[] PluginPaths { get; } = new[]
        {
            "Plugins/"
        };

        public static void LoadPlugins()
        {
            var plugins = PluginPaths.Where(Directory.Exists).SelectMany(Directory.GetFiles).Where(name => name.EndsWith(".dll")).ToList();
            foreach (var pluginPath in plugins)
            {
                // Load assembly file
                Log.Debug($"Load Plugin {pluginPath}");
                var pluginAssembly = LoadPlugin(pluginPath);
                
                // Search entry point
                foreach (Type type in pluginAssembly.GetTypes())
                {
                    if (typeof(IPlugin).IsAssignableFrom(type))
                    {
                        if (Activator.CreateInstance(type) is IPlugin plugin)
                        {
                            plugin.Register();
                            Log.Information($"Plugin {plugin.Name} by {plugin.Author} loaded");    
                        }
                        else
                        {
                            Log.Warning($"Failed to load plugin {pluginPath}/{type}");
                        }
                    }
                }
            }
        }

        private static Assembly LoadPlugin(string pluginPath)
        {
            var context = new PluginLoadContext(pluginPath);
            return context.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginPath)));
        }
    }
}