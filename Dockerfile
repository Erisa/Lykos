FROM mcr.microsoft.com/dotnet/sdk:6.0.400 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY src ./
RUN dotnet build -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0.8-alpine3.15
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Lykos.dll"]
