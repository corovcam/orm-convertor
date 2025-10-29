using Model;

namespace OrmConvertor.ServiceContracts;

public record RequiredContentDefinition(
    string OrmId,
    List<RequiredContentUnit> Required
);
