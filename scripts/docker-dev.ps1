# Script PowerShell para gestión de Docker en desarrollo

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("up", "down", "restart", "build", "logs", "migration")]
    [string]$Action
)

function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

switch ($Action) {
    "up" {
        Write-Info "Iniciando servicios de TesorosChoco..."
        docker-compose up -d
        Write-Info "Servicios iniciados. API disponible en: http://localhost:5000"
        Write-Info "SQL Server disponible en: localhost:1434"
        Write-Info "Redis disponible en: localhost:6379"
    }
    
    "down" {
        Write-Info "Deteniendo servicios de TesorosChoco..."
        docker-compose down
        Write-Info "Servicios detenidos."
    }
    
    "restart" {
        Write-Info "Reiniciando servicios de TesorosChoco..."
        docker-compose down
        docker-compose up -d
        Write-Info "Servicios reiniciados."
    }
    
    "build" {
        Write-Info "Construyendo imágenes de TesorosChoco..."
        docker-compose build --no-cache
        Write-Info "Construcción completada."
    }
    
    "logs" {
        Write-Info "Mostrando logs de los servicios..."
        docker-compose logs -f
    }
    
    "migration" {
        Write-Info "Ejecutando migraciones de Entity Framework..."
        docker-compose exec api dotnet ef database update --project /app/TesorosChoco.Infrastructure.dll --startup-project /app/TesorosChoco.API.dll
        Write-Info "Migraciones ejecutadas."
    }
}

Write-Info "Operación '$Action' completada."
