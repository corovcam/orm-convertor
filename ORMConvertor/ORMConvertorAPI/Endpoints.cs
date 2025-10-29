
using System.Collections.Generic;

namespace ORMConvertorAPI;

public static class Endpoints
{
    private static readonly AdvisorFrameworkDescriptor[] Frameworks =
    [
        new("dapper", "Dapper", "ORM", new[] { "dotnet", "sql" }),
        new("ef-core", "Entity Framework Core", "ORM", new[] { "dotnet", "sql" }),
        new("mongo-mongoose", "MongoDB / Mongoose", "ODM", new[] { "node", "document" }),
        new("neo4j-ogm", "Neo4j OGM", "ODG", new[] { "graph", "java" })
    ];

    private static readonly AdvisorMetricPreset[] Presets =
    [
        new("balanced", "Balanced", new MetricWeightsPayload(1, 1, 1, 1)),
        new("latency", "Low Latency", new MetricWeightsPayload(4, 1, 1, 1)),
        new("cost", "Cost Optimised", new MetricWeightsPayload(1, 1, 1, 3)),
        new("consistency", "Consistency Critical", new MetricWeightsPayload(1, 1, 4, 1))
    ];

    public static void MapAdvisorEndpoints(this WebApplication app)
    {
        app.MapGet("/api/advisor/frameworks", () => Results.Ok(Frameworks));
        app.MapGet("/api/advisor/metric-presets", () => Results.Ok(Presets));
    }

    private sealed record AdvisorFrameworkDescriptor(
        string Id,
        string DisplayName,
        string Category,
        IReadOnlyList<string> Tags);

    private sealed record AdvisorMetricPreset(
        string Id,
        string DisplayName,
        MetricWeightsPayload Weights);

    private sealed record MetricWeightsPayload(
        double Latency,
        double Memory,
        double Consistency,
        double Cost);
}
