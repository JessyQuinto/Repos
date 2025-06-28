# 🍫 TesorosChoco Backend

API backend para la plataforma de e-commerce de chocolates artesanales TesorosChoco, desarrollada con .NET 9 y siguiendo principios de Clean Architecture.

## 🚀 Configuración Rápida

### Prerrequisitos
- .NET 9 SDK
- Docker Desktop
- Visual Studio 2022 / VS Code (opcional)

## 🐳 Desarrollo con Docker

### Opción 1: Usar script PowerShell (Recomendado)
```powershell
# Iniciar todos los servicios
.\scripts\docker-dev.ps1 -Action up

# Ver logs en tiempo real
.\scripts\docker-dev.ps1 -Action logs

# Ejecutar migraciones
.\scripts\docker-dev.ps1 -Action migration

# Reconstruir imágenes
.\scripts\docker-dev.ps1 -Action build

# Detener servicios
.\scripts\docker-dev.ps1 -Action down
```

### Opción 2: Comandos Docker manuales
```bash
# Iniciar servicios
docker-compose up -d

# Ver logs
docker-compose logs -f

# Detener servicios
docker-compose down

# Reconstruir
docker-compose build --no-cache
```

### Servicios disponibles después del inicio:
- **API**: http://localhost:5000
- **SQL Server**: localhost:1434 (usuario: sa, password: TesorosChoco123!)
- **Redis**: localhost:6379
- **Swagger UI**: http://localhost:5000/swagger

### Instalación Automática

1. **Clona el repositorio:**
```bash
git clone <repository-url>
cd TesorosChoco.Backend
```

2. **Ejecuta el script de configuración:**
```powershell
.\setup.ps1
```

3. **Inicia la aplicación:**
```bash
dotnet run --project TesorosChoco.API
```

4. **Accede a Swagger:**
- URL: https://localhost:5001
- La documentación de la API estará disponible en la raíz

## 🏗️ Arquitectura

El proyecto sigue los principios de **Clean Architecture** con las siguientes capas:

```
📁 TesorosChoco.Backend/
├── 📁 TesorosChoco.API/          # Capa de presentación (Web API)
├── 📁 TesorosChoco.Application/  # Capa de aplicación (Casos de uso)
├── 📁 TesorosChoco.Domain/       # Capa de dominio (Entidades y reglas)
├── 📁 TesorosChoco.Infrastructure/ # Capa de infraestructura (Datos y servicios)
└── 📁 Docs/                     # Documentación del proyecto
```

## 🛠️ Tecnologías Utilizadas

- **Framework:** .NET 9
- **Base de datos:** SQL Server 2022
- **ORM:** Entity Framework Core 9
- **Cache:** Redis
- **Autenticación:** JWT Bearer Token
- **Documentación:** Swagger/OpenAPI
- **Logging:** Serilog
- **Mapping:** AutoMapper
- **Validación:** FluentValidation
- **Testing:** xUnit (pendiente)

## 🗄️ Base de Datos

### Servicios incluidos en Docker Compose:
- **SQL Server 2022:** Puerto 1433
- **Redis:** Puerto 6379

### Configuración de conexión:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=TesorosChocoDB;User Id=sa;Password=TesorosChoco123!;TrustServerCertificate=true;",
    "IdentityConnection": "Server=localhost,1433;Database=TesorosChocoIdentityDB;User Id=sa;Password=TesorosChoco123!;TrustServerCertificate=true;",
    "RedisConnection": "localhost:6379"
  }
}
```

## 🔧 Configuración Manual

Si prefieres configurar manualmente:

### 1. Iniciar servicios de base de datos:
```bash
docker-compose up -d
```

### 2. Restaurar paquetes:
```bash
dotnet restore
```

### 3. Compilar:
```bash
dotnet build
```

### 4. Ejecutar:
```bash
dotnet run --project TesorosChoco.API
```

## 📊 Endpoints Principales

### Autenticación
- `POST /api/auth/register` - Registro de usuario
- `POST /api/auth/login` - Inicio de sesión
- `POST /api/auth/refresh` - Renovar token

### Productos
- `GET /api/products` - Listar productos
- `GET /api/products/{id}` - Obtener producto
- `POST /api/products` - Crear producto (Admin)
- `PUT /api/products/{id}` - Actualizar producto (Admin)
- `DELETE /api/products/{id}` - Eliminar producto (Admin)

### Categorías
- `GET /api/categories` - Listar categorías
- `POST /api/categories` - Crear categoría (Admin)

### Carrito de Compras
- `GET /api/cart` - Obtener carrito
- `POST /api/cart/items` - Agregar item al carrito
- `PUT /api/cart/items/{id}` - Actualizar cantidad
- `DELETE /api/cart/items/{id}` - Remover item

### Órdenes
- `GET /api/orders` - Listar órdenes del usuario
- `POST /api/orders` - Crear nueva orden
- `GET /api/orders/{id}` - Obtener orden específica

## 🔐 Autenticación y Autorización

La API utiliza JWT Bearer tokens para autenticación:

1. **Registrarse o hacer login** para obtener el token
2. **Incluir el token** en el header Authorization:
   ```
   Authorization: Bearer <your-jwt-token>
   ```

### Roles disponibles:
- **User:** Usuario común (puede hacer compras)
- **Admin:** Administrador (gestión completa)
- **Producer:** Productor (gestión de sus productos)

## 🧪 Testing

```bash
# Ejecutar tests unitarios (cuando estén implementados)
dotnet test
```

## 📝 Logging

Los logs se almacenan en:
- **Consola:** Para desarrollo
- **Archivos:** `logs/log-YYYY-MM-DD.txt`

## 🐛 Solución de Problemas

### Error de conexión a SQL Server:
1. Verificar que Docker esté ejecutándose
2. Ejecutar: `docker-compose ps` para ver el estado
3. Reiniciar servicios: `docker-compose restart`

### Error de compilación:
1. Limpiar solución: `dotnet clean`
2. Restaurar paquetes: `dotnet restore`
3. Compilar: `dotnet build`

### Puerto ocupado:
1. Cambiar el puerto en `launchSettings.json`
2. O terminar el proceso que usa el puerto 5001

## 🤝 Contribución

1. Fork el proyecto
2. Crear rama para feature (`git checkout -b feature/AmazingFeature`)
3. Commit cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abrir Pull Request

## 📄 Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para detalles.

## 📞 Soporte

Si tienes problemas o preguntas:
1. Revisa la [documentación](./Docs/)
2. Crea un [issue](../../issues)
3. Contacta al equipo de desarrollo

---

**¡Disfruta desarrollando con TesorosChoco! 🍫**