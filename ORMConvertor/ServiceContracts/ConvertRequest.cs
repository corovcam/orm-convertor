using Model;

namespace OrmConvertor.ServiceContracts;

public record ConvertRequest(
    ORMEnum SourceOrm,
    ORMEnum TargetOrm,
    List<ConversionSource> Sources
);
