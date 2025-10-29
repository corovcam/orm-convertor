using System;
using System.Collections.Generic;
using Model.AbstractRepresentation.Enums;

namespace Model.AbstractRepresentation;

public class PropertyMap
{
    private readonly Dictionary<string, object?> metadata = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, object?> typeDescriptors = new(StringComparer.OrdinalIgnoreCase);
    private RelationalPropertyMetadata? relational;

    public required Property Property { get; set; }

    public Dictionary<string, object?> Metadata => metadata;

    public Dictionary<string, object?> TypeDescriptors => typeDescriptors;

    public Relation? Relation { get; set; }

    public RelationalPropertyMetadata Relational => relational ??= new RelationalPropertyMetadata(metadata);

    public string? ColumnName
    {
        get => Relational.ColumnName;
        set => Relational.ColumnName = value;
    }

    public DatabaseType? Type
    {
        get => Relational.Type;
        set => Relational.Type = value;
    }

    public int? Precision
    {
        get => Relational.Precision;
        set => Relational.Precision = value;
    }

    public int? Scale
    {
        get => Relational.Scale;
        set => Relational.Scale = value;
    }

    public int? Length
    {
        get => Relational.Length;
        set => Relational.Length = value;
    }

    public bool? IsNullable
    {
        get => Relational.IsNullable;
        set => Relational.IsNullable = value;
    }

    public Dictionary<string, string> OtherDatabaseProperties
    {
        get => Relational.OtherProperties;
        set => Relational.SetOtherProperties(value);
    }
}
