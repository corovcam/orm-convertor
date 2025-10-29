using Model;

namespace OrmConvertor.ServiceContracts.Advisor;

/// <summary>
/// Represents a single query participating in an advisor run.
/// </summary>
public record AdvisorRunQuery(
    string Id,
    ConversionSource Query,
    int Weight,
    AdvisorQueryWorkload? Workload = null
);
