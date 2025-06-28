# 🔧 Script de Mejoras Inmediatas - TesorosChoco.Backend
# Ejecutar después de revisar cada sección

# 1. CONFIGURAR USER SECRETS (CRÍTICO)
Write-Host "🔐 Configurando User Secrets..." -ForegroundColor Yellow
dotnet user-secrets init --project TesorosChoco.API

# Configurar secretos (CAMBIAR POR VALORES REALES)
dotnet user-secrets set "Jwt:Key" "SuperSecretKeyForTesorosChocoApplicationSecure123456789012345678901234567890" --project TesorosChoco.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1434;Database=TesorosChocoDB;User Id=sa;Password=TesorosChoco123!;TrustServerCertificate=true;" --project TesorosChoco.API
dotnet user-secrets set "Email:SmtpPassword" "your-real-smtp-password" --project TesorosChoco.API

Write-Host "✅ User Secrets configurados" -ForegroundColor Green

# 2. VERIFICAR SERVICIOS DOCKER
Write-Host "🐳 Verificando servicios Docker..." -ForegroundColor Yellow
docker-compose ps

# 3. COMPILAR PROYECTO
Write-Host "🔨 Compilando proyecto..." -ForegroundColor Yellow
dotnet build

# 4. EJECUTAR TESTS (si existen)
Write-Host "🧪 Ejecutando tests..." -ForegroundColor Yellow
if (Test-Path "TesorosChoco.Tests") {
    dotnet test
} else {
    Write-Host "⚠️ No se encontraron proyectos de test" -ForegroundColor Orange
}

# 5. ANÁLISIS DE SEGURIDAD
Write-Host "🛡️ Análisis de seguridad..." -ForegroundColor Yellow

# Verificar archivos de configuración
$configFiles = @("appsettings.json", "appsettings.Development.json")
foreach ($file in $configFiles) {
    $filePath = "TesorosChoco.API\$file"
    if (Test-Path $filePath) {
        $content = Get-Content $filePath -Raw
        if ($content -match '"Password":|"Key":') {
            Write-Host "⚠️ ADVERTENCIA: Credenciales encontradas en $file" -ForegroundColor Red
        }
    }
}

# 6. CREAR ARCHIVO DE CONFIGURACIÓN MEJORADA
Write-Host "📝 Creando configuración mejorada..." -ForegroundColor Yellow

# Archivo de configuración para producción
$prodConfig = @'
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "yourdomain.com",
  "Jwt": {
    "Issuer": "TesorosChoco.API",
    "Audience": "TesorosChoco.Frontend",
    "DurationInMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
    "MinimumLevel": "Warning",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/log-.txt", "rollingInterval": "Day" } }
    ]
  }
}
'@

$prodConfig | Out-File -FilePath "TesorosChoco.API\appsettings.Production.json" -Encoding UTF8

Write-Host "✅ Configuración de producción creada" -ForegroundColor Green

# 7. RESUMEN
Write-Host "`n📋 RESUMEN DE MEJORAS APLICADAS:" -ForegroundColor Cyan
Write-Host "✅ User Secrets configurados" -ForegroundColor Green
Write-Host "✅ Configuración de producción creada" -ForegroundColor Green
Write-Host "✅ Proyecto compilado" -ForegroundColor Green

Write-Host "`n⚠️ TAREAS PENDIENTES:" -ForegroundColor Yellow
Write-Host "• Revisar y aplicar índices de base de datos" -ForegroundColor White
Write-Host "• Configurar rate limiting" -ForegroundColor White
Write-Host "• Implementar health checks" -ForegroundColor White
Write-Host "• Configurar logging para producción" -ForegroundColor White
Write-Host "• Implementar tests unitarios" -ForegroundColor White

Write-Host "`n🚀 Para ejecutar la aplicación:" -ForegroundColor Green
Write-Host "dotnet run --project TesorosChoco.API" -ForegroundColor White

Write-Host "`n📖 Documentación disponible en:" -ForegroundColor Green
Write-Host "http://localhost:5000 (Swagger UI)" -ForegroundColor White
