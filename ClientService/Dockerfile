FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY ["ClientService/ClientService.csproj", "ClientService/"]

RUN dotnet restore "ClientService/ClientService.csproj"

COPY . .

WORKDIR "/src/ClientService"

# Remove any Windows-generated build artifacts copied into the image
RUN rm -rf bin obj

# Restore again inside the Linux container
RUN dotnet restore "ClientService.csproj"

RUN dotnet publish "ClientService.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM base AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ClientService.dll"]