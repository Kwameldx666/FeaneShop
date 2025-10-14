# Base image for running the service
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build image used to restore, build and publish the project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY services/auth-service/src/AuthService/AuthService.csproj AuthService/
RUN dotnet restore "AuthService/AuthService.csproj"

# Copy everything else and build/publish
COPY services/auth-service/src/AuthService/ AuthService/
WORKDIR /src/AuthService
RUN dotnet publish "AuthService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AuthService.dll"]
