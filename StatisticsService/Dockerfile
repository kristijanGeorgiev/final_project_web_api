
# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy project file and restore dependencies
COPY ["StatisticsService/StatisticsService.csproj", "StatisticsService/"]

RUN dotnet restore "StatisticsService/StatisticsService.csproj"

# Copy source code
COPY . .

WORKDIR "/src/StatisticsService"

# Remove Windows-generated build artifacts
RUN rm -rf bin obj

# Restore packages inside Linux
RUN dotnet restore "StatisticsService.csproj"

# Publish application
RUN dotnet publish "StatisticsService.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Final runtime image
FROM base AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "StatisticsService.dll"]