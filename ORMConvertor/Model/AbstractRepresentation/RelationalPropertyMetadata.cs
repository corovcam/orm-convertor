using System;
using System.Collections.Generic;
using Model.AbstractRepresentation.Enums;

namespace Model.AbstractRepresentation;

/// <summary>
/// Exposes relational-specific mapping data backed by the property metadata dictionary.
/// </summary>
public sealed class RelationalPropertyMetadata
{
    private const string ColumnKey = "relational:column";
    private const string TypeKey = "relational:dbtype";
    private const string PrecisionKey = "relational:precision";
    private const string ScaleKey = "relational:scale";
    private const string LengthKey = "relational:length";
    private const string NullableKey = "relational:isnullable";
    private const string OtherKey = "relational:other";

    private readonly Dictionary<string, object?> metadata;

    public RelationalPropertyMetadata(Dictionary<string, object?> metadata)
    {
        this.metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    }

    public string? ColumnName
    {
        get => GetString(ColumnKey);
        set => SetValue(ColumnKey, value);
    }

    public DatabaseType? Type
    {
        get => GetDatabaseType(TypeKey);
        set => SetValue(TypeKey, value);
    }

    public int? Precision
    {
        get => GetInt(PrecisionKey);
        set => SetValue(PrecisionKey, value);
    }

    public int? Scale
    {
        get => GetInt(ScaleKey);
        set => SetValue(ScaleKey, value);
    }

    public int? Length
    {
        get => GetInt(LengthKey);
        set => SetValue(LengthKey, value);
    }

    public bool? IsNullable
    {
        get => GetBool(NullableKey);
        set => SetValue(NullableKey, value);
    }

    public Dictionary<string, string> OtherProperties
        => GetOrCreateOtherProperties();

    public void SetOtherProperties(IDictionary<string, string>? values)
    {
        var target = GetOrCreateOtherProperties();
        target.Clear();
        if (values == null)
        {
            return;
        }

        foreach (var kvp in values)
        {
            target[kvp.Key] = kvp.Value;
        }
    }

    private string? GetString(string key)
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

    private int? GetInt(string key)
    {
        if (!metadata.TryGetValue(key, out var value) || value is null)
        {
            return null;
        }

        return value switch
        {
            int i => i,
            long l => (int)l,
            string s when int.TryParse(s, out var parsed) => parsed,
            _ => null
        };
    }

    private bool? GetBool(string key)
    {
        if (!metadata.TryGetValue(key, out var value) || value is null)
        {
            return null;
        }

        return value switch
        {
            bool b => b,
            string s when bool.TryParse(s, out var parsed) => parsed,
            _ => null
        };
    }

    private DatabaseType? GetDatabaseType(string key)
    {
        if (!metadata.TryGetValue(key, out var value) || value is null)
        {
            return null;
        }

        return value switch
        {
            DatabaseType dt => dt,
            string s when Enum.TryParse<DatabaseType>(s, ignoreCase: true, out var parsed) => parsed,
            int i => (DatabaseType)i,
            _ => null
        };
    }

    private Dictionary<string, string> GetOrCreateOtherProperties()
    {
        if (metadata.TryGetValue(OtherKey, out var existing) && existing is Dictionary<string, string> dict)
        {
            return dict;
        }

        var newDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        metadata[OtherKey] = newDict;
        return newDict;
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
