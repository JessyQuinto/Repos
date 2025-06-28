# Script para inicializar las migraciones de Entity Framework

Write-Host "🗄️ Inicializando migraciones de Entity Framework..." -ForegroundColor Yellow

# Verificar si estamos en el directorio correcto
if (!(Test-Path "TesorosChoco.sln")) {
    Write-Host "❌ Por favor ejecuta este script desde el directorio raíz del proyecto" -ForegroundColor Red
    exit 1
}

# Aplicar migraciones existentes
Write-Host "🔄 Aplicando migraciones a la base de datos..." -ForegroundColor Yellow
dotnet ef database update --project TesorosChoco.Infrastructure --startup-project TesorosChoco.API

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Base de datos actualizada exitosamente" -ForegroundColor Green
    Write-Host "🎉 Configuración de migraciones completada" -ForegroundColor Green
} else {
    Write-Host "❌ Error al actualizar la base de datos" -ForegroundColor Red
    exit 1
}

Write-Host "📋 Resumen:" -ForegroundColor Cyan
Write-Host "  - Base de datos: TesorosChocoDB" -ForegroundColor White
Write-Host "  - Migraciones aplicadas correctamente" -ForegroundColor White
Write-Host "  - El proyecto está listo para ejecutarse" -ForegroundColor White