using System;
using System.Collections.Generic;

namespace AdvisorBenchmarking;

internal static class BenchmarkHarnessBuilder
{
    private static readonly IReadOnlyDictionary<string, Func<BenchmarkExecutionContext, BenchmarkSource>> Generators =
        new Dictionary<string, Func<BenchmarkExecutionContext, BenchmarkSource>>(StringComparer.OrdinalIgnoreCase)
        {
            ["dapper"] = DapperBenchmarkHarnessBuilder.Build,
            ["ef-core"] = EfCoreBenchmarkHarnessBuilder.Build
        };

    public static bool SupportsFramework(string frameworkId) =>
        Generators.ContainsKey(frameworkId);

    public static BenchmarkSource Build(BenchmarkExecutionContext context)
    {
        if (!Generators.TryGetValue(context.FrameworkId, out var generator))
        {
            throw new NotSupportedException($"Benchmark harness for framework {context.FrameworkId} is not implemented yet.");
        }

        return generator(context);
    }

    public static IReadOnlyCollection<string> GetSupportedFrameworks() =>
        Generators.Keys;
}
