using Model;

namespace OrmConvertor.ServiceContracts.Advisor;

/// <summary>
/// Minimal payload for kicking off an advisor optimisation run.
/// </summary>
public record AdvisorRunRequest(
    string SourceOrmId,
    IReadOnlyList<ConversionSource> Entities,
    IReadOnlyList<AdvisorRunQuery> Queries,
    long MaxMemoryBytes,
    int MaxFrameworksToSelect,
    IReadOnlyList<string>? TargetFrameworks = null
);
