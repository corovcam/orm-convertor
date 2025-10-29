# syntax=docker/dockerfile:1.6

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ORMConvertor.sln ./
COPY OrmConvertor/OrmConvertor.csproj OrmConvertor/
COPY Model/Model.csproj Model/
COPY ServiceContracts/ServiceContracts.csproj ServiceContracts/
COPY TranslationService/TranslationService.csproj TranslationService/
RUN --mount=type=cache,target=/root/.nuget/packages dotnet restore TranslationService/TranslationService.csproj

COPY . .
RUN --mount=type=cache,target=/root/.nuget/packages dotnet restore TranslationService/TranslationService.csproj
RUN --mount=type=cache,target=/root/.nuget/packages dotnet publish TranslationService/TranslationService.csproj -c Release -o /app/out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/out .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "TranslationService.dll"]
