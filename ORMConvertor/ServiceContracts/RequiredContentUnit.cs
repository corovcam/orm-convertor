using Model;

namespace OrmConvertor.ServiceContracts;

public record RequiredContentUnit(
    int Id,
    ConversionContentType ContentType,
    string Description
);
