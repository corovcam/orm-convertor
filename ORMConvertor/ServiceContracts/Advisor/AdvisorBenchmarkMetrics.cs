
namespace OrmConvertor.ServiceContracts.Advisor;

public record AdvisorBenchmarkMetrics(
    double LatencyMs,
    long MemoryBytes,
    double ConsistencyScore,
    double MonetaryCost,
    IReadOnlyDictionary<string, double>? AdditionalMetrics = null);
