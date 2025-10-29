using Model.Metadata;

namespace Model;

public class ConversionSource
{
    public required string ContentKindId { get; init; }

    public string? Language { get; init; }

    public required string Content { get; init; }

    public ContentKindDescriptor Descriptor => ContentKindRegistry.GetById(ContentKindId);
}
