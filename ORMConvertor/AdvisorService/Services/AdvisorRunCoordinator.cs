using System.Collections.Generic;
using System.Linq;
using AdvisorBenchmarking;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Model;
using OrmConvertor;
using OrmConvertor.ServiceContracts.Advisor;
using AdvisorNamespace = Advisor.Advisor;

namespace AdvisorService.Services;

public class AdvisorRunCoordinator : IAdvisorRunCoordinator
{
    private readonly IBenchmarkExecutor benchmarkExecutor;
    private readonly ILogger<AdvisorRunCoordinator> logger;
    private readonly string connectionString;

    private static readonly string[] KnownFrameworks =
    [
        "dapper",
        "nhibernate",
        "ef-core",
        "mongo-mongoose",
        "neo4j-ogm"
    ];

    private static readonly string[] SupportedFrameworks =
    [
        "dapper",
        "ef-core",
        "mongo-mongoose",
        "neo4j-ogm"
    ];

    public AdvisorRunCoordinator(
        IBenchmarkExecutor benchmarkExecutor,
        IConfiguration configuration,
        ILogger<AdvisorRunCoordinator> logger)
    {
        this.benchmarkExecutor = benchmarkExecutor ?? throw new ArgumentNullException(nameof(benchmarkExecutor));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        connectionString = configuration.GetConnectionString("AdvisorDatabase")
            ?? configuration["Advisor:ConnectionString"]
            ?? "Server=mssql_db,1433;Database=WideWorldImporters;User ID=sa;Password=Testingorms123;TrustServerCertificate=true;";
    }

    public Task<AdvisorRunResult> RunAsync(
        AdvisorRunRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Entities);
        ArgumentNullException.ThrowIfNull(request.Queries);

        if (request.Queries.Count == 0)
        {
            throw new ArgumentException("At least one query is required", nameof(request));
        }

        logger.LogInformation("Advisor run received with {QueryCount} queries from source ORM {SourceOrm}.", request.Queries.Count, request.SourceOrmId);

        var targetFrameworks = ResolveTargetFrameworks(request);
        if (targetFrameworks.Count == 0)
        {
            throw new InvalidOperationException("No supported target frameworks resolved for advisor run. Currently supported: Dapper, EFCore.");
        }
        logger.LogInformation("Target frameworks resolved: {Frameworks}.", targetFrameworks);

        var translations = BuildTranslations(
            request,
            targetFrameworks,
            cancellationToken);
        logger.LogInformation("Translations built for all queries.");

        var measurements = RunBenchmarks(
            request,
            targetFrameworks,
            translations,
            cancellationToken);
        logger.LogInformation("Benchmarks completed for {QueryCount} queries.", request.Queries.Count);

        var result = ExecuteAdvisor(
            request,
            targetFrameworks,
            measurements);

        return Task.FromResult(result);
    }

    private static IReadOnlyList<string> ResolveTargetFrameworks(AdvisorRunRequest request)
    {
        IEnumerable<string> candidates = request.TargetFrameworks is { Count: > 0 } explicitTargets
            ? explicitTargets
            : KnownFrameworks;

        var filtered = candidates
            .Where(f => SupportedFrameworks.Contains(f))
            .Distinct()
            .ToArray();

        return filtered;
    }

    private static IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<ConversionSource>>> BuildTranslations(
        AdvisorRunRequest request,
        IReadOnlyList<string> targetFrameworks,
        CancellationToken cancellationToken)
    {
        var result = new Dictionary<string, IReadOnlyDictionary<string, IReadOnlyList<ConversionSource>>>(StringComparer.Ordinal);

        foreach (var query in request.Queries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var perFramework = new Dictionary<string, IReadOnlyList<ConversionSource>>(StringComparer.OrdinalIgnoreCase);

            foreach (var framework in targetFrameworks)
            {
                IReadOnlyList<ConversionSource> artifacts;
                if (string.Equals(framework, request.SourceOrmId, StringComparison.OrdinalIgnoreCase))
                {
                    artifacts = ComposeSources(request.Entities, query.Query);
                }
                else
                {
                    var sources = ComposeSources(request.Entities, query.Query);
                    try
                    {
                        artifacts = ConversionHandler.Convert(
                            request.SourceOrmId,
                            framework,
                            sources);
                    }
                    catch (NotSupportedException)
                    {
                        artifacts = sources;
                    }
                }

                perFramework[framework] = artifacts;
            }

            result[query.Id] = perFramework;
        }

        return result;
    }

    private static List<ConversionSource> ComposeSources(
        IReadOnlyList<ConversionSource> entities,
        ConversionSource query)
    {
        var combined = new List<ConversionSource>(entities.Count + 1);
        foreach (var entity in entities)
        {
            combined.Add(Clone(entity));
        }

        combined.Add(Clone(query));
        return combined;
    }

    private static ConversionSource Clone(ConversionSource source) =>
        new()
        {
            ContentKindId = source.ContentKindId,
            Language = source.Language,
            Content = source.Content
        };

    private IReadOnlyDictionary<string, IReadOnlyDictionary<string, BenchmarkMeasurement>> RunBenchmarks(
        AdvisorRunRequest request,
        IReadOnlyList<string> targetFrameworks,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyList<ConversionSource>>> translations,
        CancellationToken cancellationToken)
    {
        var results = new Dictionary<string, IReadOnlyDictionary<string, BenchmarkMeasurement>>(StringComparer.Ordinal);

        foreach (var query in request.Queries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogDebug("Running benchmarks for query {QueryId}.", query.Id);

            if (!translations.TryGetValue(query.Id, out var frameworkSources))
            {
                throw new InvalidOperationException($"Missing translations for query '{query.Id}'.");
            }

            var perFramework = new Dictionary<string, BenchmarkMeasurement>(StringComparer.OrdinalIgnoreCase);
            foreach (var framework in targetFrameworks)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!frameworkSources.TryGetValue(framework, out var sources))
                {
                    throw new InvalidOperationException($"Missing translation for query '{query.Id}' and framework '{framework}'.");
                }

                var metadata = BuildBenchmarkMetadata(request, query);
                var context = new BenchmarkExecutionContext(
                    framework,
                    sources,
                    connectionString,
                    query.Id,
                    metadata);
                var measurement = benchmarkExecutor.Execute(context);
                perFramework[framework] = measurement;
                logger.LogInformation(
                    "Benchmark {QueryId} on {Framework}: latency {Latency} {LatencyUnit}, memory {Memory} {MemoryUnit}, consistency {Consistency}, cost {Cost} {CostUnit}.",
                    query.Id,
                    framework,
                    measurement.Latency.Value,
                    measurement.Latency.Unit,
                    measurement.Memory.Value,
                    measurement.Memory.Unit,
                    measurement.Consistency.Value,
                    measurement.Cost.Value,
                    measurement.Cost.Unit);
            }

            results[query.Id] = perFramework;
        }

        return results;
    }

    private static IReadOnlyDictionary<string, string>? BuildBenchmarkMetadata(
        AdvisorRunRequest request,
        AdvisorRunQuery query)
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (query.Workload?.WorkloadCategory is { Length: > 0 } category)
        {
            metadata["workload-category"] = category;
        }

        if (query.Workload?.QueryShape is { Length: > 0 } shape)
        {
            metadata["query-shape"] = shape;
        }

        if (request.Workload?.ConsistencyPreference is { Length: > 0 } consistency)
        {
            metadata["consistency-preference"] = consistency;
        }

        return metadata.Count == 0 ? null : metadata;
    }

    private static AdvisorMetricWeights ResolveMetricWeights(AdvisorRunRequest request) =>
        request.Workload?.MetricWeights ?? AdvisorMetricWeights.Balanced;

    private static double ComputeWeightedCost(BenchmarkMeasurement measurement, AdvisorMetricWeights weights)
    {
        double latencyComponent = measurement.Latency.Value;
        double memoryComponent = measurement.Memory.Value / (1024d * 1024d);
        double consistencyComponent = measurement.Consistency.ToCostComponent();
        double costComponent = measurement.Cost.Value;

        double totalWeight = Math.Max(1d, weights.LatencyWeight + weights.MemoryWeight + weights.ConsistencyWeight + weights.CostWeight);

        double aggregate = (weights.LatencyWeight * latencyComponent)
            + (weights.MemoryWeight * memoryComponent)
            + (weights.ConsistencyWeight * consistencyComponent)
            + (weights.CostWeight * costComponent);

        return aggregate / totalWeight;
    }

    private static long ResolveMemoryBytes(BenchmarkMeasurement measurement) =>
        (long)Math.Max(0d, measurement.Memory.Value);

    private static AdvisorBenchmarkMetrics ConvertToContract(BenchmarkMeasurement measurement)
    {
        IReadOnlyDictionary<string, double>? additional = null;
        if (measurement.AdditionalMetrics is { Count: > 0 })
        {
            additional = measurement.AdditionalMetrics.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Value,
                StringComparer.OrdinalIgnoreCase);
        }

        return new AdvisorBenchmarkMetrics(
            measurement.Latency.Value,
            ResolveMemoryBytes(measurement),
            measurement.Consistency.Value,
            measurement.Cost.Value,
            additional);
    }

    private static AdvisorRunResult ExecuteAdvisor(
        AdvisorRunRequest request,
        IReadOnlyList<string> targetFrameworks,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, BenchmarkMeasurement>> measurements)
    {
        int queryCount = request.Queries.Count;
        int frameworkCount = targetFrameworks.Count;

        var cost = new double[queryCount * frameworkCount];
        var mem = new long[queryCount * frameworkCount];
        var weights = new int[queryCount];
        var metricWeights = ResolveMetricWeights(request);

        for (int qi = 0; qi < queryCount; qi++)
        {
            var query = request.Queries[qi];
            weights[qi] = Math.Max(1, query.Weight);
            var queryMeasurements = measurements[query.Id];

            for (int fi = 0; fi < frameworkCount; fi++)
            {
                var framework = targetFrameworks[fi];
                var measurement = queryMeasurements[framework];
                int index = (qi * frameworkCount) + fi;
                cost[index] = ComputeWeightedCost(measurement, metricWeights);
                mem[index] = ResolveMemoryBytes(measurement);
            }
        }

        int[] selected = new int[frameworkCount];
        int[] assignment = new int[queryCount];

        int status = AdvisorNamespace.Solve(
            mem,
            cost,
            weights,
            request.MaxMemoryBytes,
            request.MaxFrameworksToSelect,
            queryCount,
            frameworkCount,
            out int objective,
            selected,
            assignment);

        if (status != 0)
        {
            throw new InvalidOperationException($"Advisor solver failed with status code {status}.");
        }

        var chosenFrameworks = new List<string>();
        for (int fi = 0; fi < frameworkCount; fi++)
        {
            if (selected[fi] > 0)
            {
                chosenFrameworks.Add(targetFrameworks[fi]);
            }
        }

        var assignments = new Dictionary<string, string>(StringComparer.Ordinal);
        for (int qi = 0; qi < queryCount; qi++)
        {
            int frameworkIndex = assignment[qi];
            if (frameworkIndex < 0 || frameworkIndex >= frameworkCount)
            {
                continue;
            }

            assignments[request.Queries[qi].Id] = targetFrameworks[frameworkIndex];
        }

        var summaries = new Dictionary<string, IReadOnlyDictionary<string, AdvisorBenchmarkMetrics>>(StringComparer.Ordinal);
        foreach (var (queryId, frameworkMeasurements) in measurements)
        {
            var converted = new Dictionary<string, AdvisorBenchmarkMetrics>(StringComparer.OrdinalIgnoreCase);
            foreach (var (framework, measurement) in frameworkMeasurements)
            {
                converted[framework] = ConvertToContract(measurement);
            }

            summaries[queryId] = converted;
        }

        return new AdvisorRunResult(objective, chosenFrameworks, assignments, summaries);
    }
}
