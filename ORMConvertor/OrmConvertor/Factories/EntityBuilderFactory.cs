using AbstractWrappers;
using OrmConvertor.Plugins;

namespace OrmConvertor.Factories;

internal static class EntityBuilderFactory
{
    public static AbstractEntityBuilder Create(string ormId)
    {
        var plugin = OrmPluginRegistry.Get(ormId);
        return plugin.CreateEntityBuilder();
    }
}
