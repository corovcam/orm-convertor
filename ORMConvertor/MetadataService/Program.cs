using MetadataService.Data;
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

app.MapGet("/required-content", () => RequiredContent.GetRequiredContent)
    .WithName("RequiredContent")
    .Produces<List<RequiredContentDefinition>>();

app.MapGet("/required-content-advisor", () => RequiredContent.GetRequiredContentAdvisor)
    .WithName("RequiredContentAdvisor")
    .Produces<List<RequiredContentDefinition>>();

app.MapGet("/samples", () => Samples.GetSamples)
    .WithName("Samples")
    .Produces<Dictionary<int, string>>();

app.Run();
