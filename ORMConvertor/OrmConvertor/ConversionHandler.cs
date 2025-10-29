using AbstractWrappers;
using Model;
using OrmConvertor.Factories;

namespace OrmConvertor;

public static class ConversionHandler
{
    public static List<ConversionSource> Convert(
        string sourceOrmId,
        string targetOrmId,
        List<ConversionSource> sources
    )
    {
        var entityBuilder = EntityBuilderFactory.Create(targetOrmId);
        var queryBuilder = QueryBuilderFactory.Create(targetOrmId);

        var parsers = ParserFactory.Create(sourceOrmId, entityBuilder, queryBuilder);

        var results = new List<ConversionSource>();
        var queryParsed = false;

        foreach (var parser in parsers)
        {
            if (queryBuilder == null && parser is IQueryParser)
            {
                continue;
            }

            ConversionSource? parsable = null;

            foreach (var source in sources)
            {
                if (!ContentKindRegistry.TryGet(source.ContentKindId, out var descriptor))
                {
                    continue;
                }

                if (!parser.CanParse(descriptor, source.Language))
                {
                    continue;
                }

                parsable = source;
                break;
            }

            if (parsable == null)
            {
                continue;
            }

            if (parser is IQueryParser qp)
            {
                qp.Parse(parsable.Content, entityBuilder.EntityMap, parsable.Language);
                queryParsed = true;
            }
            else
            {
                parser.Parse(parsable.Content, parsable.Language);
            }
        }

        results.AddRange(entityBuilder.Build());
        if (queryBuilder != null && queryParsed)
        {
            results.AddRange(queryBuilder.Build());
        }

        return results;
    }
}
