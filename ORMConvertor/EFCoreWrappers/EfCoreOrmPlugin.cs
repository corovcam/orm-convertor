using AbstractWrappers;
using Model;
using Model.Metadata;

namespace EFCoreWrappers;

public class EfCoreOrmPlugin : IOrmTechnologyPlugin
{
    public OrmTechnologyDescriptor Descriptor => OrmTechnologyRegistry.GetById("ef-core");

    public AbstractEntityBuilder CreateEntityBuilder() => new EFCoreEntityBuilder();

    public AbstractQueryBuilder? CreateQueryBuilder() => null;

    public IReadOnlyCollection<IParser> CreateParsers(AbstractEntityBuilder entityBuilder, AbstractQueryBuilder? queryBuilder)
    {
        var builders = new List<IParser>
        {
            new EFCoreEntityParser(entityBuilder)
        };

        if (queryBuilder != null)
        {
            builders.Add(new EFCoreLinqQueryParser(queryBuilder));
        }

        return builders;
    }
}
