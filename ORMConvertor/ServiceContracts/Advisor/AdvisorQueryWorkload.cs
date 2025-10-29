
namespace OrmConvertor.ServiceContracts.Advisor;

public record AdvisorQueryWorkload(
    string? WorkloadCategory,
    string? QueryShape,
    int EstimatedResultSetSize);
