FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY CatalogService.Api/CatalogService.Api.csproj CatalogService.Api/
RUN dotnet restore CatalogService.Api/CatalogService.Api.csproj
COPY . .
RUN dotnet publish CatalogService.Api/CatalogService.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CatalogService.Api.dll"]
