# Base image for running the frontend
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build image used to restore, build and publish the project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY Feane.Frontend/Feane.Frontend.csproj Feane.Frontend/
RUN dotnet restore "Feane.Frontend/Feane.Frontend.csproj"

# Copy the remaining source and publish
COPY Feane.Frontend/ Feane.Frontend/
WORKDIR /src/Feane.Frontend
RUN dotnet publish "Feane.Frontend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Feane.Frontend.dll"]