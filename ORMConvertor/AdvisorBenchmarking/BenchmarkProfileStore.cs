
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace AdvisorBenchmarking;

internal sealed class BenchmarkProfileStore
{
    private readonly string profilesDirectory;
    private readonly ILogger<BenchmarkProfileStore>? logger;
    private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, BenchmarkProfileEntry>> cache = new(StringComparer.OrdinalIgnoreCase);

    public BenchmarkProfileStore(string profilesDirectory, ILogger<BenchmarkProfileStore>? logger = null)
    {
        this.profilesDirectory = profilesDirectory ?? throw new ArgumentNullException(nameof(profilesDirectory));
        this.logger = logger;
    }

    public BenchmarkProfileEntry GetProfile(string frameworkId, string profileKey)
    {
        if (string.IsNullOrWhiteSpace(frameworkId))
        {
            throw new ArgumentException("Framework identifier is required.", nameof(frameworkId));
        }

        var profiles = cache.GetOrAdd(frameworkId, LoadProfiles);
        if (profiles.TryGetValue(profileKey, out var entry))
        {
            return entry;
        }

        if (profiles.TryGetValue("default", out var @default))
        {
            return @default;
        }

        throw new InvalidOperationException($"No benchmark profile data available for framework '{frameworkId}'.");
    }

    private IReadOnlyDictionary<string, BenchmarkProfileEntry> LoadProfiles(string frameworkId)
    {
        var fileName = frameworkId.Replace('/', '-');
        var filePath = Path.Combine(profilesDirectory, $"{fileName}.json");

        string? json = null;
        if (File.Exists(filePath))
        {
            json = File.ReadAllText(filePath);
        }
        else
        {
            var resourceName = $"AdvisorBenchmarking.Profiles.{fileName}.json";
            using var resourceStream = typeof(BenchmarkProfileStore).Assembly.GetManifestResourceStream(resourceName);
            if (resourceStream != null)
            {
                using var reader = new StreamReader(resourceStream);
                json = reader.ReadToEnd();
            }
        }

        if (json is null)
        {
            logger?.LogWarning("Benchmark profile store missing data for framework {Framework}.", frameworkId);
            return new Dictionary<string, BenchmarkProfileEntry>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, BenchmarkProfileEntry>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return data is null
                ? new Dictionary<string, BenchmarkProfileEntry>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, BenchmarkProfileEntry>(data, StringComparer.OrdinalIgnoreCase);
        }
        catch (JsonException ex)
        {
            logger?.LogError(ex, "Failed to parse benchmark profile for framework {Framework}.", frameworkId);
            return new Dictionary<string, BenchmarkProfileEntry>(StringComparer.OrdinalIgnoreCase);
        }
    }

    internal sealed record BenchmarkProfileEntry(
        [property: JsonPropertyName("latencyMs")] double LatencyMs,
        [property: JsonPropertyName("memoryBytes")] long MemoryBytes,
        [property: JsonPropertyName("consistencyScore")] double ConsistencyScore,
        [property: JsonPropertyName("monetaryCost")] double MonetaryCost);
}
