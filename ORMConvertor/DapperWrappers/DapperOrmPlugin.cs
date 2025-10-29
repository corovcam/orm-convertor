using AbstractWrappers;
using Model;
using Model.Metadata;

namespace DapperWrappers;

public class DapperOrmPlugin : IOrmTechnologyPlugin
{
    public OrmTechnologyDescriptor Descriptor => OrmTechnologyRegistry.GetById("dapper");

    public AbstractEntityBuilder CreateEntityBuilder() => new DapperEntityBuilder();

    public AbstractQueryBuilder? CreateQueryBuilder() => new DapperSqlQueryBuilder();

    public IReadOnlyCollection<IParser> CreateParsers(AbstractEntityBuilder entityBuilder, AbstractQueryBuilder? queryBuilder)
        => new IParser[] { new DapperEntityParser(entityBuilder) };
}
