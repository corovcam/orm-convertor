
namespace OrmConvertor.ServiceContracts.Advisor;

public record AdvisorMetricWeights(
    double LatencyWeight,
    double MemoryWeight,
    double ConsistencyWeight,
    double CostWeight)
{
    public static AdvisorMetricWeights Balanced => new(1d, 1d, 1d, 1d);
}
