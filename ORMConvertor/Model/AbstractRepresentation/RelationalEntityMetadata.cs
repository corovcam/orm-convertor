using System;
using System.Collections.Generic;

namespace Model.AbstractRepresentation;

/// <summary>
/// Provides relational specific metadata on top of the generic entity map store.
/// </summary>
public sealed class RelationalEntityMetadata
{
    private const string TableKey = "relational:table";
    private const string SchemaKey = "relational:schema";

    private readonly Dictionary<string, object?> metadata;

    public RelationalEntityMetadata(Dictionary<string, object?> metadata)
    {
        this.metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    }

    public string? Table
    {
        get => TryGetString(TableKey);
        set => SetValue(TableKey, value);
    }

    public string? Schema
    {
        get => TryGetString(SchemaKey);
        set => SetValue(SchemaKey, value);
    }

    private string? TryGetString(string key)
    {
        if (!metadata.TryGetValue(key, out var value) || value is null)
        {
            return null;
        }

        return value switch
        {
            string s => s,
            _ => value.ToString()
        };
    }

    private void SetValue(string key, object? value)
    {
        if (value is null)
        {
            metadata.Remove(key);
            return;
        }

        metadata[key] = value;
    }
}
