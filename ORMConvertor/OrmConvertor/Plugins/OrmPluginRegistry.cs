using System.Reflection;
using System.Threading;
using AbstractWrappers;
using Model;
using Model.Metadata;

namespace OrmConvertor.Plugins;

public static class OrmPluginRegistry
{
    private static readonly Lazy<IReadOnlyDictionary<string, IOrmTechnologyPlugin>> CachedPlugins =
        new(DiscoverPlugins, LazyThreadSafetyMode.ExecutionAndPublication);

    public static IReadOnlyCollection<IOrmTechnologyPlugin> All => CachedPlugins.Value.Values.ToList();

    public static IOrmTechnologyPlugin Get(string technologyId)
    {
        if (!CachedPlugins.Value.TryGetValue(technologyId, out var plugin))
        {
            throw new KeyNotFoundException($"No plugin registered for technology '{technologyId}'.");
        }

        return plugin;
    }

    private static IReadOnlyDictionary<string, IOrmTechnologyPlugin> DiscoverPlugins()
    {
        var plugins = new Dictionary<string, IOrmTechnologyPlugin>(StringComparer.OrdinalIgnoreCase);
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            TryDiscoverInAssembly(plugins, assembly);
        }

        // Also inspect referenced assemblies that might not be loaded yet.
        var entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null)
        {
            foreach (var reference in entryAssembly.GetReferencedAssemblies())
            {
                if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == reference.Name))
                {
                    continue;
                }

                TryDiscoverInAssembly(plugins, Assembly.Load(reference));
            }
        }

        return plugins;
    }

    private static void TryDiscoverInAssembly(Dictionary<string, IOrmTechnologyPlugin> plugins, Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsAbstract || type.IsInterface)
            {
                continue;
            }

            if (!typeof(IOrmTechnologyPlugin).IsAssignableFrom(type))
            {
                continue;
            }

            if (Activator.CreateInstance(type) is not IOrmTechnologyPlugin plugin)
            {
                continue;
            }

            plugins[plugin.Descriptor.Id] = plugin;
            OrmTechnologyRegistry.Register(plugin.Descriptor);
            RegisterContentKinds(plugin.Descriptor);
        }
    }

    private static void RegisterContentKinds(OrmTechnologyDescriptor descriptor)
    {
        foreach (var kindId in descriptor.SupportedContentKinds)
        {
            if (ContentKindRegistry.TryGet(kindId, out _))
            {
                continue;
            }

            throw new InvalidOperationException(
                $"Plugin '{descriptor.Id}' references unknown content kind '{kindId}'. " +
                "Define the descriptor in metadata/content-kinds.json or register it during startup."
            );
        }
    }
}
