FROM mcr.microsoft.com/dotnet/sdk:9.0.100 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY src ./
RUN dotnet publish Lykos.csproj -c Release --property:PublishDir=$PWD/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:9.0.0-alpine3.20
LABEL com.centurylinklabs.watchtower.enable=true
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Lykos.dll"]
