using System.Collections.Concurrent;
using System.Text.Json;
using Model.Metadata;

namespace Model;

public static class OrmTechnologyRegistry
{
    private static readonly ConcurrentDictionary<string, OrmTechnologyDescriptor> Items = new(StringComparer.OrdinalIgnoreCase);
    private static bool initialized;
    private static readonly object InitLock = new();

    public static IReadOnlyCollection<OrmTechnologyDescriptor> All
    {
        get
        {
            EnsureInitialized();
            return Items.Values.ToList();
        }
    }

    public static OrmTechnologyDescriptor GetById(string id)
    {
        EnsureInitialized();
        if (!Items.TryGetValue(id, out var descriptor))
        {
            throw new KeyNotFoundException($"Unknown ORM technology '{id}'. Register the plugin before using it.");
        }

        return descriptor;
    }

    public static bool TryGet(string id, out OrmTechnologyDescriptor descriptor)
    {
        EnsureInitialized();
        return Items.TryGetValue(id, out descriptor!);
    }

    public static void Register(OrmTechnologyDescriptor descriptor)
    {
        EnsureInitialized();
        AddDescriptor(descriptor);
    }

    private static void EnsureInitialized()
    {
        if (initialized)
        {
            return;
        }

        lock (InitLock)
        {
            if (initialized)
            {
                return;
            }

            LoadDefaults();
            LoadFromConfiguration();
            initialized = true;
        }
    }

    private static void LoadDefaults()
    {
        AddDescriptor(new OrmTechnologyDescriptor(
            "dapper",
            "Dapper",
            "Micro-ORM for .NET",
            new[] { "C#" },
            new[] { "Micro-ORM", "DataMapper" },
            new[] { "Manual" },
            new[] { "csharp-entity", "csharp-query" }
        ));

        AddDescriptor(new OrmTechnologyDescriptor(
            "nhibernate",
            "NHibernate",
            "Mature ORM for .NET with XML mapping",
            new[] { "C#" },
            new[] { "ActiveRecord", "DataMapper" },
            new[] { "XML" },
            new[] { "csharp-entity", "xml-mapping" }
        ));

        AddDescriptor(new OrmTechnologyDescriptor(
            "ef-core",
            "Entity Framework Core",
            "Microsoft's ORM for .NET",
            new[] { "C#" },
            new[] { "ActiveRecord", "DataMapper" },
            new[] { "Attributes", "Fluent" },
            new[] { "csharp-entity", "csharp-query" }
        ));
    }

    private static void LoadFromConfiguration()
    {
        var metadataDirectory = Path.Combine(AppContext.BaseDirectory, "metadata");
        if (!Directory.Exists(metadataDirectory))
        {
            return;
        }

        var configurationFile = Path.Combine(metadataDirectory, "orm-technologies.json");
        if (!File.Exists(configurationFile))
        {
            return;
        }

        using var stream = File.OpenRead(configurationFile);
        var descriptors = JsonSerializer.Deserialize<List<OrmTechnologyDescriptor>>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (descriptors == null)
        {
            return;
        }

        foreach (var descriptor in descriptors)
        {
            AddDescriptor(descriptor);
        }
    }

    private static void AddDescriptor(OrmTechnologyDescriptor descriptor)
    {
        Items[descriptor.Id] = descriptor;
    }
}
