# Script de Mejoras Inmediatas - TesorosChoco.Backend
# Ejecutar despues de revisar cada seccion

# 1. CONFIGURAR USER SECRETS (CRITICO)
Write-Host "Configurando User Secrets..." -ForegroundColor Yellow
dotnet user-secrets init --project TesorosChoco.API

# Configurar secretos (CAMBIAR POR VALORES REALES)
dotnet user-secrets set "Jwt:Key" "SuperSecretKeyForTesorosChocoApplicationSecure123456789012345678901234567890" --project TesorosChoco.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1434;Database=TesorosChocoDB;User Id=sa;Password=TesorosChoco123!;TrustServerCertificate=true;" --project TesorosChoco.API

Write-Host "User Secrets configurados exitosamente" -ForegroundColor Green

# 2. VERIFICAR SERVICIOS DOCKER
Write-Host "Verificando servicios Docker..." -ForegroundColor Yellow
docker-compose ps

# 3. COMPILAR PROYECTO
Write-Host "Compilando proyecto..." -ForegroundColor Yellow
dotnet build

Write-Host ""
Write-Host "RESUMEN DE MEJORAS APLICADAS:" -ForegroundColor Cyan
Write-Host "- User Secrets configurados" -ForegroundColor Green
Write-Host "- Proyecto compilado" -ForegroundColor Green

Write-Host ""
Write-Host "TAREAS PENDIENTES:" -ForegroundColor Yellow
Write-Host "- Revisar y aplicar indices de base de datos" -ForegroundColor White
Write-Host "- Configurar rate limiting" -ForegroundColor White
Write-Host "- Implementar health checks" -ForegroundColor White
Write-Host "- Implementar tests unitarios" -ForegroundColor White

Write-Host ""
Write-Host "Para ejecutar la aplicacion:" -ForegroundColor Green
Write-Host "dotnet run --project TesorosChoco.API" -ForegroundColor White

Write-Host ""
Write-Host "Documentacion disponible en:" -ForegroundColor Green
Write-Host "http://localhost:5000 (Swagger UI)" -ForegroundColor White
