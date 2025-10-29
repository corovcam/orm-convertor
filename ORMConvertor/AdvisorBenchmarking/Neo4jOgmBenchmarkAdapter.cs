
using System;

namespace AdvisorBenchmarking;

public sealed class Neo4jOgmBenchmarkAdapter : IBenchmarkProfileAdapter
{
    public string FrameworkId => "neo4j-ogm";

    public bool CanHandle(BenchmarkExecutionContext context) =>
        string.Equals(context.FrameworkId, FrameworkId, StringComparison.OrdinalIgnoreCase);

    public string ResolveProfileKey(BenchmarkExecutionContext context)
    {
        if (context.Metadata is not null &&
            context.Metadata.TryGetValue("workload-category", out var category) &&
            !string.IsNullOrWhiteSpace(category))
        {
            return category.Trim();
        }

        if (context.Metadata is not null &&
            context.Metadata.TryGetValue("query-shape", out var queryShape) &&
            !string.IsNullOrWhiteSpace(queryShape))
        {
            return queryShape.Trim();
        }

        return "default";
    }
}
