FROM mcr.microsoft.com/dotnet/sdk:7.0.101 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY src ./
RUN dotnet build -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:7.0.0.1-alpine3.17
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Lykos.dll"]
