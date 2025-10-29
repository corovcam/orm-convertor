using Model;

namespace OrmConvertor.ServiceContracts;

public record ConvertRequest(
    string SourceOrmId,
    string TargetOrmId,
    List<ConversionSource> Sources
);
