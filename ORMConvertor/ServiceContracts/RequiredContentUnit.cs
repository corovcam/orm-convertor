using Model;

namespace OrmConvertor.ServiceContracts;

public record RequiredContentUnit(
    int Id,
    string ContentKindId,
    string Description,
    IReadOnlyList<string> Languages
);
