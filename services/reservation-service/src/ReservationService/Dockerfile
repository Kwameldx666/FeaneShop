# ============================
#   🔹 Base runtime
# ============================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 56000

# ============================
#   🔹 Build stage
# ============================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Debug
WORKDIR /src

COPY ReservationService.csproj ./
RUN dotnet restore "ReservationService.csproj"

COPY . ./
RUN dotnet publish "ReservationService.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# ============================
#   🔹 Final image
# ============================
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Development \
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_Dbg_EnableDiagnostics=1

ENTRYPOINT ["dotnet", "ReservationService.dll"]
