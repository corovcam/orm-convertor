using Model.Metadata;

namespace AbstractWrappers;

public interface IParser
{
    string Id { get; }

    bool CanParse(ContentKindDescriptor contentKind, string? language = null);

    void Parse(string source, string? language = null);
}
