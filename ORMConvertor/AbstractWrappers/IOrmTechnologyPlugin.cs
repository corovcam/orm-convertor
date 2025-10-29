using Model.Metadata;

namespace AbstractWrappers;

public interface IOrmTechnologyPlugin
{
    OrmTechnologyDescriptor Descriptor { get; }

    AbstractEntityBuilder CreateEntityBuilder();

    AbstractQueryBuilder? CreateQueryBuilder();

    IReadOnlyCollection<IParser> CreateParsers(AbstractEntityBuilder entityBuilder, AbstractQueryBuilder? queryBuilder);
}
