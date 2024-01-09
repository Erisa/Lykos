FROM mcr.microsoft.com/dotnet/sdk:8.0.101 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY src ./
RUN dotnet build -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0.1-alpine3.18
LABEL com.centurylinklabs.watchtower.enable true
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Lykos.dll"]
