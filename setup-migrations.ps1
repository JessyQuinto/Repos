# Script para inicializar las migraciones de Entity Framework

Write-Host "🗄️ Inicializando migraciones de Entity Framework..." -ForegroundColor Yellow

# Navegar al proyecto API
Set-Location "TesorosChoco.API"

# Verificar si las migraciones ya existen
$mainMigrationsExist = Test-Path "Migrations"
$identityMigrationsExist = Test-Path "IdentityMigrations"

if (-not $mainMigrationsExist) {
    Write-Host "📝 Creando migración inicial para la base de datos principal..." -ForegroundColor Yellow
    dotnet ef migrations add InitialCreate --context TesorosChocoDbContext --output-dir Migrations
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Migración principal creada exitosamente" -ForegroundColor Green
    } else {
        Write-Host "❌ Error al crear la migración principal" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "ℹ️ Las migraciones principales ya existen" -ForegroundColor Cyan
}

if (-not $identityMigrationsExist) {
    Write-Host "📝 Creando migración inicial para la base de datos de Identity..." -ForegroundColor Yellow
    dotnet ef migrations add InitialIdentityCreate --context ApplicationIdentityDbContext --output-dir IdentityMigrations
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Migración de Identity creada exitosamente" -ForegroundColor Green
    } else {
        Write-Host "❌ Error al crear la migración de Identity" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "ℹ️ Las migraciones de Identity ya existen" -ForegroundColor Cyan
}

Write-Host "`n🎉 Migraciones configuradas correctamente!" -ForegroundColor Green
Write-Host "`nPara aplicar las migraciones a la base de datos, ejecuta:" -ForegroundColor Cyan
Write-Host "dotnet ef database update --context TesorosChocoDbContext" -ForegroundColor White
Write-Host "dotnet ef database update --context ApplicationIdentityDbContext" -ForegroundColor White

# Volver al directorio raíz
Set-Location ".."
