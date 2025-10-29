using Model;

namespace OrmConvertor.ServiceContracts.Advisor;

/// <summary>
/// Minimal payload for kicking off an advisor optimisation run.
/// </summary>
public record AdvisorRunRequest(
    ORMEnum SourceOrm,
    IReadOnlyList<ConversionSource> Entities,
    IReadOnlyList<AdvisorRunQuery> Queries,
    long MaxMemoryBytes,
    int MaxFrameworksToSelect,
    IReadOnlyList<ORMEnum>? TargetFrameworks = null
);
