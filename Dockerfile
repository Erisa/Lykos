FROM mcr.microsoft.com/dotnet/sdk:6.0.401 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY src ./
RUN dotnet build -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0.9-alpine3.16
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Lykos.dll"]
