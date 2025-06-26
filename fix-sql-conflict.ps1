# Script para resolver conflicto de instancias de SQL Server
# TesorosChoco Backend - Fix SQL Server Conflict

Write-Host "🔧 Resolviendo conflicto de instancias de SQL Server..." -ForegroundColor Yellow

# Verificar procesos SQL Server existentes
Write-Host "`n📊 Verificando instancias de SQL Server..." -ForegroundColor Cyan
$sqlProcesses = Get-Process | Where-Object {$_.ProcessName -like "*sql*"}
if ($sqlProcesses) {
    Write-Host "Procesos SQL encontrados:" -ForegroundColor Green
    $sqlProcesses | Format-Table ProcessName, Id, CPU -AutoSize
} else {
    Write-Host "No se encontraron procesos SQL Server nativos." -ForegroundColor Green
}

# Verificar servicios SQL Server
Write-Host "`n🔍 Verificando servicios SQL Server..." -ForegroundColor Cyan
$sqlServices = Get-Service | Where-Object {$_.Name -like "*SQL*"}
if ($sqlServices) {
    Write-Host "Servicios SQL encontrados:" -ForegroundColor Green
    $sqlServices | Format-Table Name, Status, DisplayName -AutoSize
}

# Verificar contenedores Docker
Write-Host "`n🐳 Verificando contenedores Docker..." -ForegroundColor Cyan
$dockerContainers = docker ps --filter "name=tesoroschoco" --format "table {{.Names}}\t{{.Image}}\t{{.Status}}\t{{.Ports}}"
if ($dockerContainers) {
    Write-Host $dockerContainers -ForegroundColor Green
} else {
    Write-Host "No se encontraron contenedores TesorosChoco ejecutándose." -ForegroundColor Yellow
}

# Verificar puertos ocupados
Write-Host "`n🌐 Verificando puertos..." -ForegroundColor Cyan
$port1433 = netstat -an | findstr ":1433"
$port1434 = netstat -an | findstr ":1434"

if ($port1433) {
    Write-Host "Puerto 1433 (SQL Server nativo): OCUPADO" -ForegroundColor Red
    Write-Host $port1433
} else {
    Write-Host "Puerto 1433: Libre" -ForegroundColor Green
}

if ($port1434) {
    Write-Host "Puerto 1434 (SQL Server Docker): OCUPADO" -ForegroundColor Green
    Write-Host $port1434
} else {
    Write-Host "Puerto 1434: Libre" -ForegroundColor Yellow
}

Write-Host "`n✅ Verificación completada." -ForegroundColor Green
Write-Host "`n📋 Recomendaciones:" -ForegroundColor Yellow
Write-Host "1. Si SQL Server Express está corriendo (puerto 1433), considera detenerlo" -ForegroundColor White
Write-Host "2. Usa Docker para desarrollo (puerto 1434)" -ForegroundColor White
Write-Host "3. Las cadenas de conexión ya están configuradas para puerto 1434" -ForegroundColor White

# Mostrar configuración actual
Write-Host "`n⚙️ Configuración actual de conexión:" -ForegroundColor Cyan
Write-Host "Docker SQL Server: localhost,1434" -ForegroundColor Green
Write-Host "Redis: localhost:6379" -ForegroundColor Green
