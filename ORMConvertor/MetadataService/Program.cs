using MetadataService.Data;
using Model;
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

app.MapGet("/orm-technologies", () => OrmTechnologyRegistry.All)
    .WithName("OrmTechnologies")
    .Produces<IReadOnlyCollection<Model.Metadata.OrmTechnologyDescriptor>>();

app.MapGet("/content-kinds", () => ContentKindRegistry.All)
    .WithName("ContentKinds")
    .Produces<IReadOnlyCollection<Model.Metadata.ContentKindDescriptor>>();

app.Run();
