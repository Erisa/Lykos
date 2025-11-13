FROM --platform=${BUILDPLATFORM} \
    mcr.microsoft.com/dotnet/sdk:9.0.307 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY src ./
RUN dotnet publish Lykos.csproj -c Release --property:PublishDir=$PWD/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:9.0.11-alpine3.22
LABEL com.centurylinklabs.watchtower.enable=true
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Lykos.dll"]
