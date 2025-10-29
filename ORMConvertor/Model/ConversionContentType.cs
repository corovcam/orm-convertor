using System.Collections.Concurrent;
using System.Text.Json;
using Model.Metadata;

namespace Model;

public static class ContentKindRegistry
{
    private static readonly ConcurrentDictionary<string, ContentKindDescriptor> Items = new(StringComparer.OrdinalIgnoreCase);
    private static bool initialized;
    private static readonly object InitLock = new();

    public static IReadOnlyCollection<ContentKindDescriptor> All
    {
        get
        {
            EnsureInitialized();
            return Items.Values.ToList();
        }
    }

    public static ContentKindDescriptor GetById(string id)
    {
        EnsureInitialized();
        if (!Items.TryGetValue(id, out var descriptor))
        {
            throw new KeyNotFoundException($"Unknown content kind '{id}'. Register the descriptor before using it.");
        }

        return descriptor;
    }

    public static bool TryGet(string id, out ContentKindDescriptor descriptor)
    {
        EnsureInitialized();
        return Items.TryGetValue(id, out descriptor!);
    }

    public static void Register(ContentKindDescriptor descriptor)
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
        AddDescriptor(new ContentKindDescriptor(
            "csharp-entity",
            "C# Entity",
            new[] { "Class", "POCO" },
            new[] { "C#" },
            "C#"
        ));

        AddDescriptor(new ContentKindDescriptor(
            "csharp-query",
            "C# Query",
            new[] { "LINQ", "Method" },
            new[] { "C#" },
            "C#"
        ));

        AddDescriptor(new ContentKindDescriptor(
            "xml-mapping",
            "XML Mapping",
            new[] { "Mapping" },
            new[] { "XML" },
            "XML"
        ));
    }

    private static void LoadFromConfiguration()
    {
        var metadataDirectory = Path.Combine(AppContext.BaseDirectory, "metadata");
        if (!Directory.Exists(metadataDirectory))
        {
            return;
        }

        var configurationFile = Path.Combine(metadataDirectory, "content-kinds.json");
        if (!File.Exists(configurationFile))
        {
            return;
        }

        using var stream = File.OpenRead(configurationFile);
        var descriptors = JsonSerializer.Deserialize<List<ContentKindDescriptor>>(stream, new JsonSerializerOptions
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

    private static void AddDescriptor(ContentKindDescriptor descriptor)
    {
        Items[descriptor.Id] = descriptor;
    }
}
