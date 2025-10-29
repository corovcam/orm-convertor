using System;
using System.Collections.Generic;
using Model.AbstractRepresentation.Enums;

namespace Model.AbstractRepresentation;

/// <summary>
/// Represents a language-neutral structural element in a model graph.
/// Nodes aggregate properties and edges regardless of storage paradigm.
/// </summary>
public class Node
{
    public string Name { get; set; } = string.Empty;

    public AccessModifier? AccessModifier { get; set; }

    public string? Namespace { get; set; }

    public List<Property> Properties { get; set; } = [];

    /// <summary>
    /// Arbitrary metadata describing the node (e.g., paradigm hints, storage options).
    /// </summary>
    public Dictionary<string, object?> Metadata { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Outgoing connections from this node. Builders are expected to keep this in sync
    /// with property level relations when appropriate.
    /// </summary>
    public List<Edge> Relationships { get; } = [];
}
