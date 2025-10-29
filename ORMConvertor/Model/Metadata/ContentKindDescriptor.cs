namespace Model.Metadata;

public record ContentKindDescriptor(
    string Id,
    string DisplayName,
    IReadOnlyList<string> StatementKinds,
    IReadOnlyList<string> Languages,
    string DefaultLanguage
)
{
    public bool SupportsLanguage(string language)
        => Languages.Contains(language, StringComparer.OrdinalIgnoreCase);
}
