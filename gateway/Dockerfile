FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Feane.Gateway.Api/Feane.Gateway.Api.csproj Feane.Gateway.Api/
RUN dotnet restore Feane.Gateway.Api/Feane.Gateway.Api.csproj
COPY . .
RUN dotnet publish Feane.Gateway.Api/Feane.Gateway.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Feane.Gateway.Api.dll"]
