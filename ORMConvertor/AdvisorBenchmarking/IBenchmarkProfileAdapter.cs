namespace AdvisorBenchmarking;

public interface IBenchmarkProfileAdapter
{
    string FrameworkId { get; }

    bool CanHandle(BenchmarkExecutionContext context);

    string ResolveProfileKey(BenchmarkExecutionContext context);
}
