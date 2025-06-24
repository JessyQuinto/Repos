# TesorosChocó Backend API

Esta es la documentación técnica del backend de la plataforma de comercio electrónico TesorosChocó, desarrollada con .NET 9.

## Estructura del Proyecto

```
TesorosChoco.Backend/
├── TesorosChoco.API/           # API Controllers y configuración
│   ├── Controllers_Classes/    # Controladores de la API
│   │   ├── AuthController.cs
│   │   ├── ProductsController.cs
│   │   ├── CategoriesController.cs
│   │   ├── ProducersController.cs
│   │   ├── CartController.cs
│   │   ├── OrdersController.cs
│   │   ├── UsersController.cs
│   │   └── ContactController.cs
│   └── Extensions/            # Extensiones y configuración
├── TesorosChoco.Application/   # Lógica de aplicación
│   ├── Services_Classes/      # Servicios de aplicación
│   ├── Interfaces_Classes/    # Interfaces de servicios
│   ├── DTOs_Classes/         # Data Transfer Objects
│   └── Mappings/            # Configuración de AutoMapper
├── TesorosChoco.Domain/       # Entidades y lógica de dominio
│   ├── Entities/             # Entidades del dominio
│   └── Interfaces/          # Interfaces del dominio
└── TesorosChoco.Infrastructure/ # Acceso a datos y servicios externos
```

## Controladores Creados

### 1. AuthController (`/api/auth`)
Maneja la autenticación y autorización de usuarios.

**Endpoints:**
- `POST /login` - Autenticación de usuario
- `POST /register` - Registro de nuevo usuario
- `POST /refresh-token` - Renovación de token de acceso
- `GET /profile` - Obtener perfil del usuario autenticado
- `PUT /profile` - Actualizar perfil del usuario
- `POST /logout` - Cerrar sesión

### 2. ProductsController (`/api/products`)
Gestiona los productos del catálogo.

**Endpoints:**
- `GET /` - Obtener todos los productos
- `GET /{id}` - Obtener producto por ID
- `GET /slug/{slug}` - Obtener producto por slug
- `POST /` - Crear nuevo producto (Admin)
- `PUT /{id}` - Actualizar producto (Admin)
- `DELETE /{id}` - Eliminar producto (Admin)
- `GET /search` - Buscar productos con filtros
- `GET /featured` - Obtener productos destacados

### 3. CategoriesController (`/api/categories`)
Maneja las categorías de productos.

**Endpoints:**
- `GET /` - Obtener todas las categorías
- `GET /{id}` - Obtener categoría por ID
- `GET /slug/{slug}` - Obtener categoría por slug
- `GET /{categoryId}/products` - Obtener productos de una categoría

### 4. ProducersController (`/api/producers`)
Gestiona los productores/artesanos.

**Endpoints:**
- `GET /` - Obtener todos los productores
- `GET /{id}` - Obtener productor por ID
- `GET /featured` - Obtener productores destacados
- `GET /{producerId}/products` - Obtener productos de un productor

### 5. CartController (`/api/cart`)
Maneja el carrito de compras (requiere autenticación).

**Endpoints:**
- `GET /` - Obtener carrito del usuario
- `POST /add` - Agregar producto al carrito
- `PUT /update` - Actualizar cantidad de producto
- `DELETE /remove/{productId}` - Eliminar producto del carrito
- `DELETE /` - Vaciar carrito completamente
- `POST /sync` - Sincronizar carrito local con servidor

### 6. OrdersController (`/api/orders`)
Gestiona las órdenes de compra (requiere autenticación).

**Endpoints:**
- `POST /` - Crear nueva orden
- `GET /{id}` - Obtener orden por ID
- `GET /` - Obtener órdenes del usuario actual
- `GET /user/{userId}` - Obtener órdenes de usuario específico (Admin)
- `PATCH /{id}/status` - Actualizar estado de orden (Admin)
- `GET /all` - Obtener todas las órdenes (Admin)

### 7. UsersController (`/api/users`)
Administración de usuarios.

**Endpoints:**
- `GET /{id}` - Obtener perfil de usuario
- `PUT /{id}` - Actualizar perfil de usuario
- `GET /` - Obtener todos los usuarios (Admin)
- `DELETE /{id}` - Eliminar usuario (Admin)

### 8. ContactController (`/api/contact`) y NewsletterController (`/api/newsletter`)
Servicios de contacto y newsletter.

**Endpoints:**
- `POST /api/contact` - Enviar mensaje de contacto
- `POST /api/newsletter/subscribe` - Suscribirse al newsletter
- `DELETE /api/newsletter/unsubscribe/{email}` - Cancelar suscripción

## Características Implementadas

### Autenticación y Autorización
- JWT Token para autenticación
- Refresh Token para renovación automática
- Roles de usuario (Usuario, Admin)
- Protección de endpoints sensibles

### Manejo de Errores
- Logging estructurado con ILogger
- Respuestas de error consistentes
- Problem Details para errores HTTP
- Manejo de excepciones específicas

### Validación
- Validación de modelos con Data Annotations
- Validación de autorización en endpoints protegidos
- Verificación de ownership para recursos de usuario

### Documentación
- Swagger/OpenAPI para documentación interactiva
- Comentarios XML en controladores
- Configuración de seguridad en Swagger

### CORS
- Configuración para desarrollo local
- Soporte para múltiples orígenes de frontend
- Headers y métodos permitidos

## Servicios de Aplicación

Cada controlador utiliza servicios de aplicación que implementan la lógica de negocio:

- `AuthService` - Autenticación y gestión de tokens
- `ProductService` - Lógica de productos y búsquedas
- `CategoryService` - Gestión de categorías
- `ProducerService` - Gestión de productores
- `CartService` - Lógica del carrito de compras
- `OrderService` - Procesamiento de órdenes
- `UserService` - Administración de usuarios
- `ContactService` - Procesamiento de mensajes de contacto
- `NewsletterService` - Gestión de suscripciones

## DTOs (Data Transfer Objects)

Todos los endpoints utilizan DTOs para transferencia de datos:

- **Auth DTOs**: `LoginRequest`, `RegisterRequest`, `AuthResponse`, etc.
- **Product DTOs**: `ProductDto`, `CreateProductRequest`, `UpdateProductRequest`
- **Cart DTOs**: `CartDto`, `AddToCartRequest`, `UpdateCartRequest`
- **Order DTOs**: `OrderDto`, `CreateOrderRequest`, `UpdateOrderStatusRequest`
- **Common DTOs**: `PagedResult<T>`, `GenericResponse`

## Mapping con AutoMapper

Se utiliza AutoMapper para el mapeo entre entidades de dominio y DTOs:
- Configuración en `MappingProfile`
- Mapeo automático de propiedades
- Propiedades calculadas y navegación

## Próximos Pasos

Para completar la implementación del backend:

1. **Implementar repositorios** en la capa de Infrastructure
2. **Configurar Entity Framework** y la base de datos
3. **Implementar servicios de dominio** (TokenService, PasswordService)
4. **Agregar middleware** de autenticación JWT
5. **Configurar logging** y monitoreo
6. **Implementar validaciones** más específicas
7. **Agregar tests unitarios** y de integración
8. **Configurar deployment** para Azure

## Notas Importantes

- **No ejecutar aún**: Faltan implementaciones de repositorios y configuración de BD
- **Seguir patrones**: Clean Architecture y SOLID principles
- **Seguridad**: Implementar rate limiting y validación adicional
- **Performance**: Considerar caching y optimizaciones de consultas
- **Monitoring**: Agregar Application Insights para producción

Este backend está diseñado para ser escalable, mantenible y seguir las mejores prácticas de desarrollo con .NET.
