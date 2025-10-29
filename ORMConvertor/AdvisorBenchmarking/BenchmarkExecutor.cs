using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace AdvisorBenchmarking;

public sealed class BenchmarkExecutor : IBenchmarkExecutor
{
    private readonly IReadOnlyList<IBenchmarkRunner> runners;
    private readonly ILogger<BenchmarkExecutor>? logger;

    public BenchmarkExecutor(
        IEnumerable<IBenchmarkRunner> runners,
        ILogger<BenchmarkExecutor>? logger = null)
    {
        this.runners = runners?.ToArray() ?? throw new ArgumentNullException(nameof(runners));
        if (this.runners.Count == 0)
        {
            throw new ArgumentException("At least one benchmark runner must be registered.", nameof(runners));
        }

        this.logger = logger;
    }

    public BenchmarkMeasurement Execute(BenchmarkExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        logger?.LogInformation(
            "Benchmark start for framework {Framework} with {SourceCount} sources (QueryId: {QueryId}).",
            context.FrameworkId,
            context.Sources.Count,
            context.QueryId ?? "n/a");

        var runner = runners.FirstOrDefault(r => r.CanRun(context.FrameworkId));
        if (runner is null)
        {
            throw new NotSupportedException($"No benchmark runner registered for framework '{context.FrameworkId}'.");
        }

        var measurement = runner.Execute(context);

        logger?.LogInformation(
            "Benchmark finished for framework {Framework}. Latency {Latency} {LatencyUnit}, Memory {Memory} {MemoryUnit}, Cost {Cost} {CostUnit}.",
            context.FrameworkId,
            measurement.Latency.Value,
            measurement.Latency.Unit,
            measurement.Memory.Value,
            measurement.Memory.Unit,
            measurement.Cost.Value,
            measurement.Cost.Unit);

        return measurement;
    }
}
