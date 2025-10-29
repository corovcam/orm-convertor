using System;
using System.Collections.Generic;
using AbstractWrappers;
using Model;
using Model.Metadata;

namespace NodeMongoosePlugin;

public sealed class NodeMongooseOrmPlugin : IOrmTechnologyPlugin
{
    public OrmTechnologyDescriptor Descriptor => OrmTechnologyRegistry.GetById("node-mongoose");

    public AbstractEntityBuilder CreateEntityBuilder() => new UnsupportedEntityBuilder();

    public AbstractQueryBuilder? CreateQueryBuilder() => null;

    public IReadOnlyCollection<IParser> CreateParsers(AbstractEntityBuilder entityBuilder, AbstractQueryBuilder? queryBuilder)
        => Array.Empty<IParser>();

    private sealed class UnsupportedEntityBuilder : AbstractEntityBuilder
    {
        public override List<ConversionSource> Build()
        {
            throw new NotSupportedException("Node.js Mongoose generation is not implemented yet.");
        }

        protected override void BuildForeignKey() => throw new NotSupportedException();

        protected override void BuildImports() => throw new NotSupportedException();

        protected override void BuildPrimaryKey() => throw new NotSupportedException();

        protected override void BuildProperties() => throw new NotSupportedException();

        protected override void BuildTableSchema() => throw new NotSupportedException();

        protected override void FinalizeBuild() => throw new NotSupportedException();
    }
}
