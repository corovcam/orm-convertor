# syntax=docker/dockerfile:1.6

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS advisor-native
WORKDIR /src/advisor
RUN apt-get update && apt-get install -y build-essential libglpk-dev \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /artifacts
COPY Advisor/ilp.c .
RUN gcc -fPIC -shared -o /artifacts/libadvisor.so ilp.c -lglpk

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ORMConvertor.sln ./
COPY AdvisorService/AdvisorService.csproj AdvisorService/
COPY Advisor/Advisor.csproj Advisor/
COPY AdvisorBenchmarking/AdvisorBenchmarking.csproj AdvisorBenchmarking/
COPY OrmConvertor/OrmConvertor.csproj OrmConvertor/
COPY Model/Model.csproj Model/
COPY ServiceContracts/ServiceContracts.csproj ServiceContracts/
RUN --mount=type=cache,target=/root/.nuget/packages dotnet restore AdvisorService/AdvisorService.csproj

COPY . .
RUN --mount=type=cache,target=/root/.nuget/packages dotnet restore AdvisorService/AdvisorService.csproj
COPY --from=advisor-native /artifacts/libadvisor.so Advisor/libadvisor.so
RUN --mount=type=cache,target=/root/.nuget/packages dotnet publish AdvisorService/AdvisorService.csproj -c Release -o /app/out --no-restore \
    && cp Advisor/libadvisor.so /app/out/

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y libglpk40 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/out .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "AdvisorService.dll"]
