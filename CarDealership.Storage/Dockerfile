FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 5229
EXPOSE 7292

ENV ASPNETCORE_ENVIRONMENT=Development

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["CarDealership.Storage/nuget.config", "./"]

COPY ./nupkgs /nupkgs
COPY ["CarDealership.Storage/CarDealership.Storage.csproj", "CarDealership.Storage/"]
RUN dotnet restore "./CarDealership.Storage/CarDealership.Storage.csproj" --configfile nuget.config

COPY . .
WORKDIR "/src/CarDealership.Storage"
RUN dotnet build "./CarDealership.Storage.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CarDealership.Storage.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CarDealership.Storage.dll"]