namespace Weikio.PluginFramework.Microsoft.DependencyInjection;

public class DefaultPluginOption
{
    public Func<IServiceProvider, IEnumerable<Type>, Type> DefaultType { get; set; }
        = (serviceProvider, implementingTypes) => implementingTypes.FirstOrDefault();
}