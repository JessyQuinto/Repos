#!/bin/bash

# Script para ejecutar migraciones de Entity Framework en Docker

echo "Esperando a que SQL Server esté listo..."
sleep 30

echo "Ejecutando migraciones de Entity Framework..."
dotnet ef database update --project TesorosChoco.Infrastructure --startup-project TesorosChoco.API --verbose

echo "Migraciones completadas."
echo "Iniciando aplicación..."
exec dotnet TesorosChoco.API.dll
