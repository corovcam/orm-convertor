FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GatewayAPI.csproj", "./"]
RUN dotnet restore "GatewayAPI.csproj"
COPY . .
RUN dotnet publish "GatewayAPI.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GatewayAPI.dll"]
