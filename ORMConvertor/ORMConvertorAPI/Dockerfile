# syntax=docker/dockerfile:1.6

# Stage 1: Build the Angular frontend
FROM node:22-alpine AS angular-build
WORKDIR /frontend

COPY ORMConvertorAPI/frontend/package*.json ./
RUN --mount=type=cache,target=/root/.npm npm ci

COPY ORMConvertorAPI/frontend .
RUN npx ng build --configuration production --base-href / --deploy-url / --output-path=dist

# Stage 2: Build and publish the gateway
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dotnet-build
WORKDIR /app

COPY ORMConvertor.sln ./
COPY ORMConvertorAPI/ORMConvertorAPI.csproj ORMConvertorAPI/
COPY ServiceContracts/ServiceContracts.csproj ServiceContracts/
RUN --mount=type=cache,target=/root/.nuget/packages dotnet restore ORMConvertorAPI/ORMConvertorAPI.csproj

COPY . .
RUN --mount=type=cache,target=/root/.nuget/packages dotnet restore ORMConvertorAPI/ORMConvertorAPI.csproj

COPY --from=angular-build /frontend/dist/browser ORMConvertorAPI/wwwroot
RUN --mount=type=cache,target=/root/.nuget/packages dotnet publish ORMConvertorAPI/ORMConvertorAPI.csproj -c Release -o /app/out --no-restore

# Stage 3: Create runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=dotnet-build /app/out .
EXPOSE 5072
ENV ASPNETCORE_URLS=http://+:5072
ENTRYPOINT ["dotnet", "ORMConvertorAPI.dll"]
