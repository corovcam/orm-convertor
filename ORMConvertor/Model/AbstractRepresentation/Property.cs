using System;
using System.Collections.Generic;
using Model.AbstractRepresentation.Enums;

namespace Model.AbstractRepresentation;

public class Property
{
    public required string Name { get; set; }

    public required CLRTypeModel Type { get; set; }

    public bool IsNullable { get; set; } = false;

    public AccessModifier? AccessModifier { get; set; }

    public List<string> OtherModifiers { get; set; } = [];

    public bool HasGetter { get; set; } = false;

    public bool HasSetter { get; set; } = false;

    public string? DefaultValue { get; set; }

    /// <summary>
    /// Provider-neutral descriptors such as JSON schema fragments or CLR annotations.
    /// </summary>
    public Dictionary<string, object?> TypeDescriptors { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Additional property annotations gathered during parsing.
    /// </summary>
    public Dictionary<string, object?> Annotations { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Traversal hints (e.g., graph navigation info) keyed by provider.
    /// </summary>
    public Dictionary<string, object?> TraversalMetadata { get; } = new(StringComparer.OrdinalIgnoreCase);
}
