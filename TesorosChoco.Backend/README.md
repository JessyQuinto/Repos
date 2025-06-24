# TesorosChocó Backend

## Descripción
Este proyecto es el backend para la aplicación TesorosChocó, desarrollado utilizando .NET 9. La arquitectura sigue el patrón Clean Architecture, separando las diferentes capas de la aplicación para una mejor organización y mantenibilidad.

## Estructura del Proyecto
- **TesorosChoco.Domain**: Contiene la lógica de negocio y las entidades del dominio.
- **TesorosChoco.Application**: Maneja la lógica de aplicación, incluyendo comandos y consultas.
- **TesorosChoco.Infrastructure**: Implementa el acceso a datos y las integraciones con servicios externos.
- **TesorosChoco.API**: Proporciona la interfaz de programación de aplicaciones (API) para interactuar con el backend.

## Requisitos
- .NET 9
- SQL Server (para la base de datos)

## Instalación
1. Clona el repositorio.
2. Navega al directorio del proyecto.
3. Ejecuta `dotnet restore` para restaurar los paquetes NuGet.
4. Configura la cadena de conexión en `appsettings.json`.
5. Ejecuta `dotnet build` para compilar el proyecto.

## Ejecución
Para ejecutar la API, navega al directorio `TesorosChoco.API` y ejecuta:
```
dotnet run
```

## Contribuciones
Las contribuciones son bienvenidas. Por favor, abre un issue o envía un pull request para discutir cambios.

## Licencia
Este proyecto está bajo la licencia MIT.