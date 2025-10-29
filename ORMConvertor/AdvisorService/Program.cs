using AdvisorBenchmarking;
using AdvisorService.Services;
using Microsoft.Extensions.Logging;
using OrmConvertor.ServiceContracts.Advisor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(provider =>
{
    var logger = provider.GetService<ILogger<BenchmarkProfileStore>>();
    return new BenchmarkProfileStore(AppContext.BaseDirectory, logger);
});
builder.Services.AddSingleton<IBenchmarkProfileAdapter, MongoMongooseBenchmarkAdapter>();
builder.Services.AddSingleton<IBenchmarkProfileAdapter, Neo4jOgmBenchmarkAdapter>();
builder.Services.AddSingleton<IBenchmarkRunner, RoslynBenchmarkRunner>();
builder.Services.AddSingleton<IBenchmarkRunner, ProfiledBenchmarkRunner>();
builder.Services.AddSingleton<IBenchmarkExecutor, BenchmarkExecutor>();
builder.Services.AddSingleton<IAdvisorRunCoordinator, AdvisorRunCoordinator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/advisor/test", (AdvisorSolveRequest req) =>
{
    try
    {
        int[] selected = new int[req.F];
        int[] assignment = new int[req.Q];
        int status = Advisor.Advisor.Solve(
            req.Memory,
            req.Cost,
            req.Z,
            req.MEM,
            req.N,
            req.Q,
            req.F,
            out int objective,
            selected,
            assignment
        );
        var response = new AdvisorSolveResponse(
            status,
            objective,
            (int[])selected.Clone(),
            (int[])assignment.Clone()
        );
        return Results.Ok(response);
    }
    catch (Exception e)
    {
        return Results.BadRequest(e.Message);
    }
})
.WithName("AdvisorTest")
.Produces<AdvisorSolveResponse>()
.ProducesProblem(StatusCodes.Status400BadRequest);

app.MapPost("/advisor/run", async (
    AdvisorRunRequest req,
    IAdvisorRunCoordinator coordinator,
    CancellationToken cancellationToken) =>
{
    try
    {
        var result = await coordinator.RunAsync(req, cancellationToken);
        return Results.Ok(result);
    }
    catch (Exception e)
    {
        return Results.BadRequest(e.Message);
    }
})
.WithName("AdvisorRun")
.Produces<AdvisorRunResult>()
.ProducesProblem(StatusCodes.Status400BadRequest);

app.Run();
