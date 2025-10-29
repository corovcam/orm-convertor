# syntax=docker/dockerfile:1.6

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ORMConvertor.sln ./
COPY MetadataService/MetadataService.csproj MetadataService/
COPY Model/Model.csproj Model/
COPY SampleData/SampleData.csproj SampleData/
COPY ServiceContracts/ServiceContracts.csproj ServiceContracts/
RUN --mount=type=cache,target=/root/.nuget/packages dotnet restore MetadataService/MetadataService.csproj

COPY . .
RUN --mount=type=cache,target=/root/.nuget/packages dotnet restore MetadataService/MetadataService.csproj
RUN --mount=type=cache,target=/root/.nuget/packages dotnet publish MetadataService/MetadataService.csproj -c Release -o /app/out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/out .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "MetadataService.dll"]
