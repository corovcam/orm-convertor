using AbstractWrappers;
using OrmConvertor.Plugins;

namespace OrmConvertor.Factories;

internal static class ParserFactory
{
    public static IReadOnlyCollection<IParser> Create(string ormId, AbstractEntityBuilder entityBuilder, AbstractQueryBuilder? queryBuilder)
    {
        var plugin = OrmPluginRegistry.Get(ormId);
        return plugin.CreateParsers(entityBuilder, queryBuilder);
    }
}
