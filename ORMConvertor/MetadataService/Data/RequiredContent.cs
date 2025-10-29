using System.Linq;
using OrmConvertor.ServiceContracts;
using Model;

namespace MetadataService.Data;

public static class RequiredContent
{
    public static List<RequiredContentDefinition> GetRequiredContent =>
    [
        BuildDefinition("dapper", new[] { "csharp-entity" }),
        BuildDefinition("nhibernate", new[] { "csharp-entity", "xml-mapping" }),
        BuildDefinition("ef-core", new[] { "csharp-entity", "csharp-query" }),
        BuildDefinition("java-hibernate", new[] { "java-entity" }),
        BuildDefinition("python-sqlalchemy", new[] { "python-model" }),
        BuildDefinition("node-mongoose", new[] { "typescript-schema" })
    ];

    public static List<RequiredContentDefinition> GetRequiredContentAdvisor =>
    [
        BuildDefinition("dapper", new[] { "csharp-entity" }),
        BuildDefinition("nhibernate", new[] { "csharp-entity", "xml-mapping" }),
        BuildDefinition("ef-core", new[] { "csharp-entity", "csharp-query", "csharp-query", "csharp-query" }),
        BuildDefinition("java-hibernate", new[] { "java-entity" }),
        BuildDefinition("python-sqlalchemy", new[] { "python-model" }),
        BuildDefinition("node-mongoose", new[] { "typescript-schema" })
    ];

    private static RequiredContentDefinition BuildDefinition(string ormId, IEnumerable<string> contentKinds)
    {
        var units = contentKinds
            .Select((kind, index) => CreateUnit(index + 1, kind))
            .ToList();
        return new RequiredContentDefinition(ormId, units);
    }

    private static RequiredContentUnit CreateUnit(int id, string contentKindId)
    {
        var descriptor = ContentKindRegistry.GetById(contentKindId);
        return new RequiredContentUnit(id, contentKindId, descriptor.DisplayName, descriptor.Languages);
    }
}
