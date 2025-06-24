# Script para configurar la infraestructura de TesorosChoco

Write-Host "🍫 Configurando TesorosChoco Backend..." -ForegroundColor Yellow

# Verificar si Docker está ejecutándose
try {
    docker --version | Out-Null
    Write-Host "✅ Docker está disponible" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker no está disponible. Por favor instala Docker Desktop" -ForegroundColor Red
    exit 1
}

# Iniciar servicios de base de datos con Docker Compose
Write-Host "🚀 Iniciando servicios de base de datos..." -ForegroundColor Yellow
docker-compose up -d

# Esperar a que SQL Server esté listo
Write-Host "⏳ Esperando a que SQL Server esté listo..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Verificar conexión a SQL Server
$maxRetries = 10
$retryCount = 0
$connected = $false

while ($retryCount -lt $maxRetries -and -not $connected) {
    try {
        $connectionString = "Server=localhost,1433;User Id=sa;Password=TesorosChoco123!;TrustServerCertificate=true;"
        $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
        $connection.Open()
        $connection.Close()
        $connected = $true
        Write-Host "✅ Conexión a SQL Server establecida" -ForegroundColor Green
    } catch {
        $retryCount++
        Write-Host "⏳ Intento $retryCount de $maxRetries - Esperando conexión a SQL Server..." -ForegroundColor Yellow
        Start-Sleep -Seconds 5
    }
}

if (-not $connected) {
    Write-Host "❌ No se pudo conectar a SQL Server después de $maxRetries intentos" -ForegroundColor Red
    exit 1
}

# Restaurar paquetes NuGet
Write-Host "📦 Restaurando paquetes NuGet..." -ForegroundColor Yellow
dotnet restore

# Compilar la solución
Write-Host "🔨 Compilando la solución..." -ForegroundColor Yellow
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Compilación exitosa" -ForegroundColor Green
    
    Write-Host "`n🎉 Configuración completada!" -ForegroundColor Green
    Write-Host "`nPuedes ejecutar la aplicación con:" -ForegroundColor Cyan
    Write-Host "dotnet run --project TesorosChoco.API" -ForegroundColor White
    Write-Host "`nO usar el comando:" -ForegroundColor Cyan
    Write-Host "dotnet watch run --project TesorosChoco.API" -ForegroundColor White
    Write-Host "`nSwagger estará disponible en: https://localhost:5001" -ForegroundColor Cyan
} else {
    Write-Host "❌ Error en la compilación" -ForegroundColor Red
    Write-Host "Revisa los errores mostrados arriba" -ForegroundColor Yellow
}

Write-Host "`n📊 Estado de los servicios:" -ForegroundColor Yellow
docker-compose ps
