
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace AdvisorBenchmarking;

public sealed class ProfiledBenchmarkRunner : IBenchmarkRunner
{
    private readonly BenchmarkProfileStore profileStore;
    private readonly IReadOnlyDictionary<string, IBenchmarkProfileAdapter> adapters;
    private readonly ILogger<ProfiledBenchmarkRunner>? logger;

    public ProfiledBenchmarkRunner(
        BenchmarkProfileStore profileStore,
        IEnumerable<IBenchmarkProfileAdapter> adapters,
        ILogger<ProfiledBenchmarkRunner>? logger = null)
    {
        this.profileStore = profileStore ?? throw new ArgumentNullException(nameof(profileStore));
        if (adapters is null)
        {
            throw new ArgumentNullException(nameof(adapters));
        }

        this.adapters = new Dictionary<string, IBenchmarkProfileAdapter>(StringComparer.OrdinalIgnoreCase);
        foreach (var adapter in adapters)
        {
            if (!this.adapters.TryAdd(adapter.FrameworkId, adapter))
            {
                throw new InvalidOperationException($"Duplicate benchmark adapter registration for framework '{adapter.FrameworkId}'.");
            }
        }

        this.logger = logger;
    }

    public bool CanRun(string frameworkId) =>
        adapters.ContainsKey(frameworkId);

    public BenchmarkMeasurement Execute(BenchmarkExecutionContext context)
    {
        if (!adapters.TryGetValue(context.FrameworkId, out var adapter) || !adapter.CanHandle(context))
        {
            throw new NotSupportedException($"No profiled benchmark adapter registered for framework '{context.FrameworkId}'.");
        }

        var profileKey = adapter.ResolveProfileKey(context);
        logger?.LogDebug("Resolving benchmark profile for framework {Framework} using key {Key}.", context.FrameworkId, profileKey);
        var entry = profileStore.GetProfile(context.FrameworkId, profileKey);

        return new BenchmarkMeasurement(
            new BenchmarkMetricValue("latency", entry.LatencyMs, "ms"),
            new BenchmarkMetricValue("memory", entry.MemoryBytes, "bytes"),
            new BenchmarkMetricValue("consistency", entry.ConsistencyScore, "score", LowerIsBetter: false),
            new BenchmarkMetricValue("monetary-cost", entry.MonetaryCost, "usd"));
    }
}
