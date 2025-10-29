using System.Collections.Generic;
using Model;

namespace AdvisorBenchmarking;

public sealed record BenchmarkExecutionContext(
    string FrameworkId,
    IReadOnlyList<ConversionSource> Sources,
    string ConnectionString,
    string? QueryId = null,
    IReadOnlyDictionary<string, string>? Metadata = null);
