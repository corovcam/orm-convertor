using AbstractWrappers;
using Model;
using NHibernateWrappers;
using SampleData;

namespace Tests.NHibernate;

public class NHibernateToNHibernateTest
{
    [Fact]
    public void NHibernateToNHibernateOverall()
    {
        AbstractEntityBuilder builder = new NHibernateEntityBuilder();
        var entityParser = new NHibernateEntityParser(builder);
        var mappingParser = new NHibernateXMLMappingParser(builder);
        entityParser.Parse(CustomerSampleNHibernate.Entity);
        mappingParser.Parse(CustomerSampleNHibernate.XmlMapping);

        var results = builder.Build();
        var entityOutput = results.Single(x => x.ContentKindId == "csharp-entity");
        var xmlOutput = results.Single(x => x.ContentKindId == "xml-mapping");

        Assert.Multiple(() =>
        {
            Assert.Equal("csharp-entity", entityOutput.ContentKindId);
            Assert.Equal(CustomerSampleNHibernate.Entity, entityOutput.Content, ignoreLineEndingDifferences: true);

            Assert.Equal("xml-mapping", xmlOutput.ContentKindId);
            Assert.Equal(CustomerSampleNHibernate.XmlMapping, xmlOutput.Content, ignoreLineEndingDifferences: true);
        });
    }
}
