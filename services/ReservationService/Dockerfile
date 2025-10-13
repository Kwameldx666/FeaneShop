FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ReservationService.Api/ReservationService.Api.csproj ReservationService.Api/
RUN dotnet restore ReservationService.Api/ReservationService.Api.csproj
COPY . .
RUN dotnet publish ReservationService.Api/ReservationService.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ReservationService.Api.dll"]
