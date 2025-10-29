using OrmConvertor;
using OrmConvertor.ServiceContracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/convert", (ConvertRequest req) =>
{
    try
    {
        var converted = ConversionHandler.Convert(req.SourceOrm, req.TargetOrm, req.Sources);
        return Results.Ok(new ConvertResponse(converted));
    }
    catch (Exception e)
    {
        return Results.BadRequest(e.Message);
    }
})
.WithName("Convert")
.Produces<ConvertResponse>()
.ProducesProblem(StatusCodes.Status400BadRequest);

app.Run();
