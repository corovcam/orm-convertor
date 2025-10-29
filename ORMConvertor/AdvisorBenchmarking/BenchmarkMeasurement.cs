using System.Collections.Generic;

namespace AdvisorBenchmarking;

public sealed record BenchmarkMeasurement(
    BenchmarkMetricValue Latency,
    BenchmarkMetricValue Memory,
    BenchmarkMetricValue Consistency,
    BenchmarkMetricValue Cost,
    IReadOnlyDictionary<string, BenchmarkMetricValue>? AdditionalMetrics = null);

public sealed record BenchmarkMetricValue(
    string Name,
    double Value,
    string Unit,
    bool LowerIsBetter = true)
{
    public double ToCostComponent()
    {
        if (LowerIsBetter)
        {
            return Value;
        }

        var normalized = Math.Clamp(Value, 0d, 1d);
        return 1d - normalized;
    }
}
