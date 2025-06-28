# Dockerfile para TesorosChoco API
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Instalar herramientas de Entity Framework
RUN dotnet tool install --global dotnet-ef

# Copiar archivos de proyecto y restaurar dependencias
COPY ["TesorosChoco.API/TesorosChoco.API.csproj", "TesorosChoco.API/"]
COPY ["TesorosChoco.Application/TesorosChoco.Application.csproj", "TesorosChoco.Application/"]
COPY ["TesorosChoco.Domain/TesorosChoco.Domain.csproj", "TesorosChoco.Domain/"]
COPY ["TesorosChoco.Infrastructure/TesorosChoco.Infrastructure.csproj", "TesorosChoco.Infrastructure/"]

RUN dotnet restore "TesorosChoco.API/TesorosChoco.API.csproj"

# Copiar todo el código fuente
COPY . .

# Compilar la aplicación
WORKDIR "/src/TesorosChoco.API"
RUN dotnet build "TesorosChoco.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TesorosChoco.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Instalar curl para health checks y .NET SDK para migraciones EF
RUN apt-get update && apt-get install -y curl wget

# Instalar .NET SDK para ejecutar migraciones EF
RUN wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y dotnet-sdk-9.0

COPY --from=publish /app/publish .

# Crear directorio para logs
RUN mkdir -p /app/logs

# Copiar script de entrada
COPY scripts/docker-entrypoint.sh /usr/local/bin/
RUN chmod +x /usr/local/bin/docker-entrypoint.sh

ENTRYPOINT ["docker-entrypoint.sh"]
