using System;
using System.Collections.Generic;
using Model.AbstractRepresentation.Enums;

namespace Model.AbstractRepresentation;

/// <summary>
/// Represents a paradigm-agnostic connection between nodes.
/// </summary>
public class Edge
{
    public string? Name { get; set; }

    public string? Source { get; set; }

    public required string Target { get; set; }

    public required Cardinality Cardinality { get; set; }

    /// <summary>
    /// Additional metadata for the edge (e.g., join conditions, traversal hints).
    /// </summary>
    public Dictionary<string, object?> Metadata { get; } = new(StringComparer.OrdinalIgnoreCase);
}
