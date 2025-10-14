# Base image for running the gateway
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build image used to restore, build and publish the project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY FeaneGateway/FeaneGateway.csproj FeaneGateway/
RUN dotnet restore "FeaneGateway/FeaneGateway.csproj"

# Copy the remaining source and publish
COPY FeaneGateway/ FeaneGateway/
WORKDIR /src/FeaneGateway
RUN dotnet publish "FeaneGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FeaneGateway.dll"]
