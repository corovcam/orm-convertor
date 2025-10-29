using System;
using System.Collections.Generic;
using Model;

namespace AdvisorBenchmarking;

internal static class BenchmarkHarnessBuilder
{
    private static readonly IReadOnlyDictionary<string, Func<IReadOnlyList<ConversionSource>, string, BenchmarkSource>> Generators =
        new Dictionary<string, Func<IReadOnlyList<ConversionSource>, string, BenchmarkSource>>(StringComparer.OrdinalIgnoreCase)
        {
            ["dapper"] = DapperBenchmarkHarnessBuilder.Build,
            ["ef-core"] = EfCoreBenchmarkHarnessBuilder.Build
        };

    public static BenchmarkSource Build(
        string frameworkId,
        IReadOnlyList<ConversionSource> sources,
        string connectionString)
    {
        if (!Generators.TryGetValue(frameworkId, out var generator))
        {
            throw new NotSupportedException($"Benchmark harness for framework {frameworkId} is not implemented yet.");
        }

        return generator(sources, connectionString);
    }
}

