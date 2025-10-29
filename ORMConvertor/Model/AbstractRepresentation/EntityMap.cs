using System;
using System.Collections.Generic;

namespace Model.AbstractRepresentation;

public class EntityMap
{
    private Node node;
    private RelationalEntityMetadata? relational;

    public EntityMap()
    {
        node = new Entity();
        Metadata["storage.paradigm"] = "relational";
    }

    public Node Node
    {
        get => node;
        set => node = value ?? throw new ArgumentNullException(nameof(value));
    }

    public Entity Entity
    {
        get
        {
            if (node is Entity entity)
            {
                return entity;
            }

            var fallback = new Entity
            {
                Name = node.Name,
                Namespace = node.Namespace,
                AccessModifier = node.AccessModifier,
                Properties = node.Properties
            };

            foreach (var kvp in node.Metadata)
            {
                fallback.Metadata[kvp.Key] = kvp.Value;
            }

            foreach (var edge in node.Relationships)
            {
                fallback.Relationships.Add(edge);
            }

            node = fallback;
            return fallback;
        }
        set => node = value ?? throw new ArgumentNullException(nameof(value));
    }

    public Dictionary<string, object?> Metadata { get; } = new(StringComparer.OrdinalIgnoreCase);

    public DocumentSchema? DocumentSchema { get; set; }

    public List<PropertyMap> PropertyMaps { get; set; } = [];

    public RelationalEntityMetadata Relational => relational ??= new RelationalEntityMetadata(Metadata);

    public string? Table
    {
        get => Relational.Table;
        set => Relational.Table = value;
    }

    public string? Schema
    {
        get => Relational.Schema;
        set => Relational.Schema = value;
    }
}
