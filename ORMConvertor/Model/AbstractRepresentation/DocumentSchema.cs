using System;
using System.Collections.Generic;

namespace Model.AbstractRepresentation;

/// <summary>
/// Represents a document-oriented schema definition with language-neutral descriptors.
/// </summary>
public class DocumentSchema
{
    public DocumentSchema()
    {
        Metadata["storage.paradigm"] = "document";
    }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Fields inside the document schema.
    /// </summary>
    public List<Property> Fields { get; set; } = [];

    /// <summary>
    /// JSON schema fragments or other descriptors keyed by provider.
    /// </summary>
    public Dictionary<string, object?> TypeDescriptors { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Arbitrary metadata related to document storage (indexes, shard keys, etc.).
    /// </summary>
    public Dictionary<string, object?> Metadata { get; } = new(StringComparer.OrdinalIgnoreCase);
}
