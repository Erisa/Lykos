FROM --platform=${BUILDPLATFORM} \
    mcr.microsoft.com/dotnet/sdk:9.0.201 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY src ./
RUN dotnet publish Lykos.csproj -c Release --property:PublishDir=$PWD/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:9.0.3-alpine3.21
LABEL com.centurylinklabs.watchtower.enable=true
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Lykos.dll"]
