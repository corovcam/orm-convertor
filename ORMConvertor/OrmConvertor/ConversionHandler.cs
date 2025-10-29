using System;
using System.Collections.Generic;
using System.Linq;
using AbstractWrappers;
using Model;
using Model.AbstractRepresentation;
using Model.Metadata;
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
        var sourceDescriptor = OrmTechnologyRegistry.GetById(sourceOrmId);
        var targetDescriptor = OrmTechnologyRegistry.GetById(targetOrmId);

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

        var context = BuildTranslationContext(entityBuilder.EntityMap);
        ValidateParadigms(sourceDescriptor, targetDescriptor, context);

        results.AddRange(entityBuilder.Build());
        if (queryBuilder != null && queryParsed)
        {
            results.AddRange(queryBuilder.Build());
        }

        return results;
    }
    private static TranslationContext BuildTranslationContext(EntityMap entityMap)
    {
        var edges = new List<Edge>();
        if (entityMap.Node != null)
        {
            edges.AddRange(entityMap.Node.Relationships);
        }

        foreach (var relation in entityMap.PropertyMaps.Select(pm => pm.Relation).Where(r => r != null))
        {
            if (!edges.Contains(relation!))
            {
                edges.Add(relation!);
            }
        }

        return new TranslationContext(
            entityMap.Node,
            entityMap.PropertyMaps.AsReadOnly(),
            edges.AsReadOnly(),
            entityMap.DocumentSchema
        );
    }

    private static void ValidateParadigms(
        OrmTechnologyDescriptor source,
        OrmTechnologyDescriptor target,
        TranslationContext context)
    {
        var structureCategories = ExtractStructureCategories(context, source);
        var targetCategories = MapToCategories(target.Paradigms)
            .Union(MapToCategories(target.Serializers))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (targetCategories.Count == 0)
        {
            targetCategories.Add("Relational");
        }

        if (!structureCategories.Overlaps(targetCategories))
        {
            throw new InvalidOperationException(
                $"Translation from '{source.Id}' paradigms [{string.Join(", ", structureCategories)}] " +
                $"to '{target.Id}' paradigms [{string.Join(", ", targetCategories)}] is not supported."
            );
        }
    }

    private static HashSet<string> ExtractStructureCategories(
        TranslationContext context,
        OrmTechnologyDescriptor sourceDescriptor)
    {
        var categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (context.DocumentSchema != null)
        {
            categories.Add("Document");
            if (context.DocumentSchema.Metadata.TryGetValue("storage.paradigm", out var docParadigm))
            {
                AddCategoriesFromValue(categories, docParadigm as string);
            }
        }

        if (context.Node.Metadata.TryGetValue("storage.paradigm", out var paradigmValue))
        {
            AddCategoriesFromValue(categories, paradigmValue as string);
        }

        if (categories.Count == 0)
        {
            foreach (var paradigm in sourceDescriptor.Paradigms)
            {
                AddCategoriesFromValue(categories, paradigm);
            }
        }

        if (categories.Count == 0)
        {
            categories.Add("Relational");
        }

        return categories;
    }

    private static HashSet<string> MapToCategories(IEnumerable<string> values)
    {
        var categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var value in values)
        {
            AddCategoriesFromValue(categories, value);
        }

        return categories;
    }

    private static void AddCategoriesFromValue(ISet<string> categories, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (value.Contains("graph", StringComparison.OrdinalIgnoreCase))
        {
            categories.Add("Graph");
        }

        if (value.Contains("document", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("odm", StringComparison.OrdinalIgnoreCase))
        {
            categories.Add("Document");
        }

        if (value.Contains("relational", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("orm", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("record", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("mapper", StringComparison.OrdinalIgnoreCase))
        {
            categories.Add("Relational");
        }
    }

    private sealed record TranslationContext(
        Node Node,
        IReadOnlyList<PropertyMap> PropertyMaps,
        IReadOnlyList<Edge> Edges,
        DocumentSchema? DocumentSchema);

}
