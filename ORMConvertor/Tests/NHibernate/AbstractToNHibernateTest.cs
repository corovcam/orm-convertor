using AbstractWrappers;
using Model;
using NHibernateWrappers;
using SampleData;

namespace Tests.NHibernate;

public class AbstractToNHibernateTest
{
    [Fact]
    public void AbstractToNHibernateOverall()
    {
        var source = CustomerSampleNHibernate.Map;

        AbstractEntityBuilder builder = new NHibernateEntityBuilder
        {
            EntityMap = source
        };

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
