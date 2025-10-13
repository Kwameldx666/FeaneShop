# ASP.NET Core reverse proxy gateway
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["services/gateway/src/GatewayService/GatewayService.csproj", "services/gateway/src/GatewayService/"]
RUN dotnet restore "services/gateway/src/GatewayService/GatewayService.csproj"
COPY . .
WORKDIR "/src/services/gateway/src/GatewayService"
RUN dotnet build "GatewayService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "GatewayService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GatewayService.dll"]
