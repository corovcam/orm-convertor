
namespace OrmConvertor.ServiceContracts.Advisor;

public record AdvisorWorkloadCharacteristics(
    int ConcurrentUsers,
    double ReadPercentage,
    double WritePercentage,
    string ConsistencyPreference,
    AdvisorMetricWeights MetricWeights);
