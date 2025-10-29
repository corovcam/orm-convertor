namespace Model.Metadata;

public record OrmTechnologyDescriptor(
    string Id,
    string DisplayName,
    string? Description,
    IReadOnlyList<string> Languages,
    IReadOnlyList<string> Paradigms,
    IReadOnlyList<string> Serializers,
    IReadOnlyList<string> SupportedContentKinds
)
{
    public bool SupportsContentKind(string contentKindId)
        => SupportedContentKinds.Contains(contentKindId, StringComparer.OrdinalIgnoreCase);
}
