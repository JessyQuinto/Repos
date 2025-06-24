
# Guía de Integración Frontend-Backend

## Resumen

Esta guía documenta la integración completa entre el frontend React y el backend para la plataforma de comercio electrónico "Tesoros del Chocó".

## Estructura de Archivos

### Servicios API
- `src/services/apiClient.ts` - Cliente HTTP base con manejo de autenticación y errores
- `src/services/authService.ts` - Servicio de autenticación
- `src/services/productService.ts` - Servicio de productos
- `src/services/cartService.ts` - Servicio de carrito
- `src/services/orderService.ts` - Servicio de pedidos

### Hooks API
- `src/hooks/api/useAuth.ts` - Hook de autenticación con React Query
- `src/hooks/api/useProducts.ts` - Hooks para gestión de productos
- `src/hooks/api/useCart.ts` - Hook para gestión de carrito
- `src/hooks/api/useOrders.ts` - Hooks para gestión de pedidos

### Utilidades
- `src/utils/tokenManager.ts` - Gestión de tokens JWT
- `src/utils/errorHandler.ts` - Manejo centralizado de errores
- `src/hooks/useNotifications.ts` - Sistema de notificaciones
- `src/hooks/useCartSync.ts` - Sincronización carrito local/servidor

### Configuración
- `src/config/apiConfig.ts` - Configuración de endpoints y constantes
- `src/types/api.ts` - Tipos TypeScript para APIs
- `.env.development` - Variables de entorno para desarrollo
- `.env.production` - Variables de entorno para producción

## Configuración Inicial

### 1. Variables de Entorno

Crear archivo `.env.local` basado en `.env.development`:

```bash
cp .env.development .env.local
```

Actualizar `VITE_API_BASE_URL` con la URL correcta del backend.

### 2. Configuración del Cliente API

El cliente API está configurado para:
- Manejo automático de tokens JWT
- Refresh automático de tokens
- Reintentos en caso de errores de red
- Timeout configurable
- Manejo de errores centralizados

### 3. Integración con React Query

Todos los hooks utilizan React Query para:
- Cache inteligente de datos
- Sincronización en background
- Optimistic updates
- Manejo de estados de carga

## Uso de los Servicios

### Autenticación

```typescript
import { useAuth } from '@/hooks/api/useAuth';

function LoginComponent() {
  const { login, isLoggingIn, user, isAuthenticated } = useAuth();
  
  const handleLogin = async (credentials) => {
    await login(credentials);
  };
}
```

### Productos

```typescript
import { useFeaturedProducts, useProductBySlug } from '@/hooks/api/useProducts';

function ProductComponent() {
  const { data: products, isLoading } = useFeaturedProducts();
  const { data: product } = useProductBySlug('collar-tradicional');
}
```

### Carrito

```typescript
import { useCart } from '@/hooks/api/useCart';

function CartComponent() {
  const { items, addToCart, removeFromCart, total } = useCart();
}
```

### Sincronización de Carrito

El sistema incluye sincronización automática entre carrito local (para usuarios no autenticados) y carrito del servidor (para usuarios autenticados).

```typescript
import { useCartSync } from '@/hooks/useCartSync';

function App() {
  const cart = useCartSync(); // Maneja automáticamente la sincronización
}
```

## Manejo de Errores

### Tipos de Error

1. **NetworkError** - Problemas de conectividad
2. **ApiError** - Errores de la API (400, 401, 500, etc.)
3. **AuthError** - Errores de autenticación específicos

### Uso del Error Handler

```typescript
import { ErrorHandler } from '@/utils/errorHandler';

try {
  await someApiCall();
} catch (error) {
  const errorInfo = ErrorHandler.handleError(error);
  
  if (errorInfo.shouldRetry) {
    // Mostrar botón de reintentar
  }
  
  if (errorInfo.type === 'auth') {
    // Redirigir a login
  }
}
```

## Sistema de Notificaciones

```typescript
import { useNotifications } from '@/hooks/useNotifications';

function Component() {
  const { showSuccess, showError } = useNotifications();
  
  const handleAction = async ()

 => {
    try {
      await someAction();
      showSuccess('¡Acción completada!');
    } catch (error) {
      showError('Error al completar la acción');
    }
  };
}
```

## Testing

### Prueba de Conexión

Para probar la conectividad con el backend:

```typescript
// Test de conexión básica
curl http://localhost:3001/api/health

// Test de autenticación
curl -X POST http://localhost:3001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"password"}'
```

### Endpoints Requeridos

El backend debe implementar los siguientes endpoints:

#### Autenticación
- `POST /api/auth/login` - Iniciar sesión
- `POST /api/auth/register` - Registrarse
- `POST /api/auth/refresh` - Renovar token
- `GET /api/auth/profile` - Obtener perfil
- `PUT /api/auth/profile` - Actualizar perfil

#### Productos
- `GET /api/products` - Listar productos
- `GET /api/products/featured` - Productos destacados
- `GET /api/products/slug/:slug` - Producto por slug
- `GET /api/products/search` - Buscar productos

#### Carrito
- `GET /api/cart` - Obtener carrito
- `POST /api/cart/add` - Agregar al carrito
- `PUT /api/cart/update` - Actualizar cantidad
- `DELETE /api/cart/remove/:id` - Eliminar del carrito
- `POST /api/cart/sync` - Sincronizar carrito

#### Pedidos
- `POST /api/orders` - Crear pedido
- `GET /api/orders` - Listar pedidos del usuario
- `GET /api/orders/:id` - Obtener pedido específico

## Migración de Datos Mock

Para migrar completamente de datos mock a APIs:

1. Reemplazar imports de `productApi.ts` por hooks de `useProducts.ts`
2. Actualizar componentes para usar hooks de React Query
3. Implementar manejo de estados de carga y error
4. Configurar variables de entorno
5. Probar conectividad con backend

## Estructura Sugerida del Backend

```
backend/
├── controllers/
│   ├── authController.js
│   ├── productController.js
│   ├── cartController.js
│   └── orderController.js
├── middleware/
│   ├── auth.js
│   └── errorHandler.js
├── models/
│   ├── User.js
│   ├── Product.js
│   ├── Cart.js
│   └── Order.js
├── routes/
│   ├── auth.js
│   ├── products.js
│   ├── cart.js
│   └── orders.js
└── server.js
```

## Próximos Pasos

1. Implementar backend con los endpoints documentados
2. Configurar base de datos
3. Implementar autenticación JWT
4. Configurar CORS para el frontend
5. Implementar manejo de archivos para imágenes
6. Configurar sistema de pagos
7. Implementar notificaciones en tiempo real
8. Agregar analytics y métricas

## Troubleshooting

### Problemas Comunes

1. **CORS Error** - Configurar CORS en el backend
2. **Token Expired** - El sistema maneja automáticamente el refresh
3. **Network Error** - Verificar conectividad y URL del API
4. **401 Unauthorized** - Verificar implementación de autenticación

### Logs de Debug

El sistema incluye logging extensivo. Para habilitar debug:

```bash
# En .env.local
VITE_DEBUG_MODE=true
VITE_LOG_LEVEL=debug
```

## Contacto y Soporte

Para soporte con la integración, consulta:
- Documentación del API
- Logs del navegador (Console)
- Logs del servidor backend
- Estado de React Query DevTools
```
