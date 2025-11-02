# ---------- Базовый рантайм ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# ---------- Этап сборки ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копируем csproj и восстанавливаем зависимости
COPY FeaneGateway.csproj .
RUN dotnet restore "FeaneGateway.csproj"

# Копируем оставшиеся файлы и пу��ликуем
COPY . .
RUN dotnet publish "FeaneGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# ---------- Финальный образ ----------
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FeaneGateway.dll"]
