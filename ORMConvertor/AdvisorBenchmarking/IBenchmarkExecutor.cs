using Model;

namespace AdvisorBenchmarking;

public interface IBenchmarkExecutor
{
    BenchmarkMeasurement Execute(
        string frameworkId,
        IReadOnlyList<ConversionSource> sources,
        string connectionString);
}
