using Model;

namespace OrmConvertor.ServiceContracts.Advisor;

/// <summary>
/// Minimal advisor response containing the recommended framework selection.
/// </summary>
public record AdvisorRunResult(
    int Objective,
    IReadOnlyList<string> SelectedFrameworks,
    IReadOnlyDictionary<string, string> QueryAssignments
);
