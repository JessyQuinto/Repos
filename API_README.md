# TesorosChoco Backend API

API REST para la plataforma de comercio electrónico "Tesoros del Chocó" desarrollada con .NET 9 y Entity Framework Core.

## Características

- **Autenticación JWT** con refresh tokens
- **CORS** configurado para múltiples frontends
- **Swagger/OpenAPI** para documentación interactiva
- **Logging** con Serilog
- **Validación** con FluentValidation
- **AutoMapper** para mapeo de DTOs
- **Problem Details** para manejo estandarizado de errores
- **Health Check** endpoint

## Estructura del Proyecto

```
TesorosChoco.Backend/
├── TesorosChoco.API/           # Capa de presentación (Controllers, Middlewares)
├── TesorosChoco.Application/   # Capa de aplicación (Services, DTOs, Interfaces)
├── TesorosChoco.Domain/        # Capa de dominio (Entities, ValueObjects, Enums)
└── TesorosChoco.Infrastructure/ # Capa de infraestructura (Data, Repositories)
```

## Requisitos

- .NET 9 SDK
- SQL Server (LocalDB o instancia completa)
- Visual Studio 2022 o VS Code

## Configuración

### 1. Clonar el repositorio
```bash
git clone <repository-url>
cd TesorosChoco.Backend
```

### 2. Configurar base de datos
Actualizar la cadena de conexión en `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TesorosChocoDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 3. Ejecutar migraciones
```bash
# Desde la raíz del proyecto
dotnet ef database update --project TesorosChoco.Infrastructure --startup-project TesorosChoco.API
```

### 4. Restaurar paquetes y compilar
```bash
dotnet restore
dotnet build
```

## Ejecución

### Modo Desarrollo
```bash
cd TesorosChoco.API
dotnet run
```

La API estará disponible en:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:7001`
- Swagger UI: `https://localhost:7001`

### Modo Producción
```bash
dotnet run --environment Production
```

## Endpoints Principales

### Autenticación
- `POST /api/auth/login` - Iniciar sesión
- `POST /api/auth/register` - Registrar usuario
- `POST /api/auth/refresh` - Renovar token
- `POST /api/auth/logout` - Cerrar sesión

### Productos
- `GET /api/products` - Lista de productos
- `GET /api/products/{id}` - Producto por ID
- `GET /api/products/featured` - Productos destacados
- `GET /api/products/search` - Búsqueda con filtros
- `POST /api/products` - Crear producto (Admin)
- `PUT /api/products/{id}` - Actualizar producto (Admin)
- `DELETE /api/products/{id}` - Eliminar producto (Admin)

### Categorías
- `GET /api/categories` - Lista de categorías
- `GET /api/categories/{id}` - Categoría por ID
- `GET /api/categories/{id}/products` - Productos por categoría

### Productores
- `GET /api/producers` - Lista de productores
- `GET /api/producers/{id}` - Productor por ID
- `GET /api/producers/{id}/products` - Productos por productor

### Carrito (Requiere autenticación)
- `GET /api/cart` - Obtener carrito del usuario
- `POST /api/cart` - Actualizar carrito
- `DELETE /api/cart` - Vaciar carrito

### Órdenes (Requiere autenticación)
- `POST /api/orders` - Crear orden
- `GET /api/orders` - Órdenes del usuario
- `GET /api/orders/{id}` - Orden por ID
- `GET /api/orders/user/{userId}` - Órdenes por usuario (Admin)
- `PATCH /api/orders/{id}/status` - Actualizar estado (Admin)

### Usuarios (Requiere autenticación)
- `GET /api/users/{id}` - Perfil de usuario
- `PUT /api/users/{id}` - Actualizar perfil

### Contacto
- `POST /api/contact` - Enviar mensaje de contacto

### Newsletter
- `POST /api/newsletter/subscribe` - Suscribirse al newsletter
- `DELETE /api/newsletter/unsubscribe/{email}` - Cancelar suscripción

### Health Check
- `GET /api/health` - Estado de la API

## Configuración CORS

La API está configurada para aceptar requests desde:
- `http://localhost:3000` (React)
- `https://localhost:3000` (React HTTPS)
- `http://localhost:8080` (Vite)
- `https://localhost:8080` (Vite HTTPS)
- `http://localhost:5173` (Vite alternativo)
- `https://localhost:5173` (Vite alternativo HTTPS)

## Autenticación

La API usa JWT tokens con refresh tokens:

1. **Login**: `POST /api/auth/login` retorna access token y refresh token
2. **Autorización**: Incluir `Authorization: Bearer {access_token}` en headers
3. **Renovación**: Usar `POST /api/auth/refresh` con refresh token cuando expire

### Ejemplo de uso:
```javascript
// Login
const response = await fetch('/api/auth/login', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ email: 'user@example.com', password: 'password' })
});

const { user, token } = await response.json();

// Usar token en requests posteriores
const productsResponse = await fetch('/api/products', {
  headers: { 'Authorization': `Bearer ${token}` }
});
```

## Base de Datos

La API utiliza Entity Framework Core con SQL Server. Las migraciones se aplican automáticamente al iniciar la aplicación en desarrollo.

### Entidades principales:
- `User` - Usuarios del sistema
- `Product` - Productos del catálogo
- `Category` - Categorías de productos
- `Producer` - Productores/artesanos
- `Cart` / `CartItem` - Carrito de compras
- `Order` / `OrderItem` - Órdenes de compra
- `ContactMessage` - Mensajes de contacto
- `NewsletterSubscription` - Suscripciones al newsletter

## Logging

La aplicación usa Serilog para logging. Los logs se guardan en:
- Consola (Development)
- Archivos en `logs/` (Production)

## Testing

```bash
# Ejecutar tests (cuando estén disponibles)
dotnet test
```

## Deployment

### Docker
```bash
# Construir imagen
docker build -t tesoroschoco-api .

# Ejecutar contenedor
docker run -p 8080:80 tesoroschoco-api
```

### Azure App Service
1. Configurar cadena de conexión en Azure
2. Configurar variables de entorno
3. Usar pipeline de CI/CD

## Troubleshooting

### Problemas comunes:

1. **Error de conexión a BD**: Verificar cadena de conexión y que SQL Server esté corriendo
2. **CORS errors**: Verificar que la URL del frontend esté en la configuración CORS
3. **JWT errors**: Verificar configuración de claves secretas en `appsettings.json`

### Logs
Revisar logs en:
- Consola durante desarrollo
- Archivos en `TesorosChoco.API/logs/` en producción

## Contribución

1. Fork del repositorio
2. Crear branch para feature: `git checkout -b feature/nueva-funcionalidad`
3. Commit cambios: `git commit -m 'Agregar nueva funcionalidad'`
4. Push al branch: `git push origin feature/nueva-funcionalidad`
5. Crear Pull Request

## Licencia

[Especificar licencia]
