using AbstractWrappers;
using OrmConvertor.Plugins;

namespace OrmConvertor.Factories;

internal static class QueryBuilderFactory
{
    public static AbstractQueryBuilder? Create(string ormId)
    {
        var plugin = OrmPluginRegistry.Get(ormId);
        return plugin.CreateQueryBuilder();
    }
}
