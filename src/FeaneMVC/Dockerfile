# See https://aka.ms/customizecontainer to learn how to customize your debug container
# This stage is used when running from Visual Studio in fast mode (default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build stage used to restore, build, and publish the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy csproj files and restore as distinct layers
COPY ["src/FeaneMVC/FeaneMVC.csproj", "FeaneMVC/"]
COPY ["src/FeaneMVC.Application/FeaneMVC.Application.csproj", "FeaneMVC.Application/"]
COPY ["src/FeaneMVC.Domain/FeaneMVC.Domain.csproj", "FeaneMVC.Domain/"]
COPY ["src/FeaneMVC.Infrastructure/FeaneMVC.Infrastructure.csproj", "FeaneMVC.Infrastructure/"]
RUN dotnet restore "FeaneMVC/FeaneMVC.csproj"

# Copy the remaining source
COPY src/FeaneMVC/ FeaneMVC/
COPY src/FeaneMVC.Application/ FeaneMVC.Application/
COPY src/FeaneMVC.Domain/ FeaneMVC.Domain/
COPY src/FeaneMVC.Infrastructure/ FeaneMVC.Infrastructure/
WORKDIR /src/FeaneMVC

# Build and publish
RUN dotnet publish "FeaneMVC.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FeaneMVC.dll"]
