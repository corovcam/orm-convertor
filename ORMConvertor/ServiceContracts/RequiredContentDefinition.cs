using Model;

namespace OrmConvertor.ServiceContracts;

public record RequiredContentDefinition(
    ORMEnum OrmType,
    List<RequiredContentUnit> Required
);
