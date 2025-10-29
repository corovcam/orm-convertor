namespace OrmConvertor.ServiceContracts.Advisor;

public record AdvisorSolveRequest(
    long[] Memory,
    double[] Cost,
    int[] Z,
    long MEM,
    int N,
    int Q,
    int F
);
