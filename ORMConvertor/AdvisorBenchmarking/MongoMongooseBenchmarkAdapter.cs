
using System;

namespace AdvisorBenchmarking;

public sealed class MongoMongooseBenchmarkAdapter : IBenchmarkProfileAdapter
{
    public string FrameworkId => "mongo-mongoose";

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

        return "default";
    }
}
