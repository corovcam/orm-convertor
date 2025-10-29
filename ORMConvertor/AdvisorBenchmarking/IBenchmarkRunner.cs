namespace AdvisorBenchmarking;

public interface IBenchmarkRunner
{
    bool CanRun(string frameworkId);

    BenchmarkMeasurement Execute(BenchmarkExecutionContext context);
}
