using AbstractWrappers;
using Model;
using Model.Metadata;

namespace NHibernateWrappers;

public class NHibernateOrmPlugin : IOrmTechnologyPlugin
{
    public OrmTechnologyDescriptor Descriptor => OrmTechnologyRegistry.GetById("nhibernate");

    public AbstractEntityBuilder CreateEntityBuilder() => new NHibernateEntityBuilder();

    public AbstractQueryBuilder? CreateQueryBuilder() => null;

    public IReadOnlyCollection<IParser> CreateParsers(AbstractEntityBuilder entityBuilder, AbstractQueryBuilder? queryBuilder)
        => new IParser[]
        {
            new NHibernateEntityParser(entityBuilder),
            new NHibernateXMLMappingParser(entityBuilder)
        };
}
