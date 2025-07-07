# 🍫 Documento de Integración Backend-Frontend para Proyecto E-commerce TesorosChoco

## 📋 Propósito

Este documento tiene como finalidad guiar la integración entre el backend y el frontend del sistema e-commerce TesorosChoco, priorizando el desarrollo backend. Se describen los servicios, rutas, controladores y otros elementos relevantes para que el equipo frontend pueda implementar correctamente cada funcionalidad siguiendo un flujo paso a paso.

## 🔧 Configuración Base

- **URL Base (Desarrollo Local)**: `https://localhost:5001` / `http://localhost:5000`
- **URL Base (Docker)**: `http://localhost:5002`
- **Versionado API**: `api/v1/`
- **Content-Type**: `application/json`
- **Autenticación**: JWT Bearer## 🚀 Funcionalidades Faltantes por Implementar

### 📋 **Funcionalidades Críticas del Flujo Básico**

Después de analizar el código, identifiqué las siguientes funcionalidades **esenciales** que faltan para completar el flujo básico de un e-commerce:

#### **🔐 Sistema de Confirmación de Email**
- `POST /api/v1/auth/confirm-email` - Confirmar email de usuario
- `POST /api/v1/auth/resend-confirmation` - Reenviar email de confirmación
- **Impacto**: Los usuarios se registran pero no pueden confirmar su email
- **Estado actual**: Servicio implementado pero **sin endpoints expuestos**

#### **📧 Notificaciones de Orden**
- **Email de confirmación de orden** al completar checkout
- **Email de cambio de estado** cuando admin actualiza estado
- **Impacto**: Los usuarios no reciben confirmación de sus pedidos
- **Estado actual**: Servicio de email implementado pero **no se usa en el flujo**

#### **💳 Procesamiento de Pagos**
- **Validación de métodos de pago**
- **Integración con pasarelas** (aunque sea simulada)
- **Estado de pago en órdenes**
- **Impacto**: Las órdenes se crean como "Pending" pero no hay proceso de pago
- **Estado actual**: Solo se guarda el método, **no hay procesamiento**

#### **🔄 Estados de Orden Automáticos**
- **Transiciones de estado automatizadas**
- **Validaciones de cambio de estado**
- **Triggers de notificaciones por estado**
- **Impacto**: Estados se cambian manualmente sin lógica de negocio
- **Estado actual**: Solo cambio manual por admin

#### **📱 Proceso Completo de Checkout**
**Lo que falta después del checkout:**
1. ✅ Crear orden ← **Implementado**
2. ❌ Procesar pago ← **Falta**
3. ❌ Enviar email de confirmación ← **Falta**
4. ❌ Actualizar estado a "Processing" ← **Falta**
5. ❌ Liberar stock reservado ← **Parcialmente implementado**

---

### 🎯 **Prioridades para Demo Funcional:**

1. **🔐 Confirmación de email** (crítico para flujo de usuario)
2. **📧 Emails de confirmación de orden** (esencial para e-commerce)
3. **💳 Simulación básica de pago** (completar checkout)
4. **🔄 Estados automáticos de orden** (mejorar experiencia admin)ken
- **Documentación**: Swagger UI disponible en `/swagger`

---

## 🚀 Flujo de Integración por Etapas

### **Etapa 1: Registro de Usuario**

#### **Endpoint**: `POST /api/v1/auth/register`

**Descripción**: Crea un nuevo usuario en el sistema.

**Body esperado**:
```json
{
  "firstName": "Juan",
  "lastName": "Pérez",
  "email": "juan.perez@email.com",
  "password": "MiPassword123!",
  "phone": "+57 300 123 4567",
  "address": "Calle 123 #45-67, Bogotá"
}
```

**Respuesta exitosa (201)**:
```json
{
  "success": true,
  "message": "Registration successful",
  "data": {
    "user": {
      "id": 1,
      "firstName": "Juan",
      "lastName": "Pérez",
      "email": "juan.perez@email.com",
      "phone": "+57 300 123 4567",
      "address": "Calle 123 #45-67, Bogotá",
      "role": "User",
      "isActive": true,
      "createdAt": "2025-06-30T10:00:00Z"
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_string",
    "tokenType": "Bearer",
    "expiresIn": 3600
  }
}
```

**Errores posibles**:
- **409 Conflict**: Email ya registrado
- **400 Bad Request**: Validación de campos fallida

**Validaciones**:
- Email debe ser único y tener formato válido
- Password debe tener al menos 8 caracteres, una mayúscula, una minúscula y un número
- FirstName y LastName son requeridos
- Phone y address son opcionales pero recomendados

---

### **Etapa 2: Inicio de Sesión**

#### **Endpoint**: `POST /api/v1/auth/login`

**Descripción**: Autentica al usuario y retorna token JWT para acceso a funcionalidades protegidas.

**Body esperado**:
```json
{
  "email": "juan.perez@email.com",
  "password": "MiPassword123!"
}
```

**Respuesta exitosa (200)**:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "user": {
      "id": 1,
      "firstName": "Juan",
      "lastName": "Pérez",
      "email": "juan.perez@email.com",
      "phone": "+57 300 123 4567",
      "address": "Calle 123 #45-67, Bogotá",
      "role": "User",
      "isActive": true
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_string",
    "tokenType": "Bearer",
    "expiresIn": 3600,
    "refreshTokenExpiresIn": 604800
  }
}
```

**Errores posibles**:
- **401 Unauthorized**: Credenciales incorrectas
- **404 Not Found**: Usuario no encontrado
- **400 Bad Request**: Datos de entrada inválidos

**⚠️ Importante**: El token debe incluirse en todas las peticiones protegidas:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

### **Etapa 3: Exploración del Catálogo**

#### **Endpoint**: `GET /api/v1/products`

**Descripción**: Lista todos los productos disponibles con opciones de filtrado y paginación.

**Query Parameters disponibles**:
- `featured` (bool): Productos destacados
- `categoryId` (int): Filtrar por categoría
- `producerId` (int): Filtrar por productor
- `searchTerm` (string): Búsqueda por nombre/descripción
- `page` (int, default: 1): Número de página
- `pageSize` (int, default: 10, max: 100): Elementos por página

**Ejemplo de request**:
```
GET /api/v1/products?featured=true&page=1&pageSize=12
```

**Respuesta exitosa (200)**:
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Chocolate Negro 70%",
      "slug": "chocolate-negro-70",
      "description": "Delicioso chocolate negro artesanal con 70% de cacao colombiano...",
      "price": 15000,
      "discountedPrice": 12000,
      "currentPrice": 12000,
      "hasDiscount": true,
      "image": "https://example.com/chocolate1.jpg",
      "images": [
        "https://example.com/chocolate1.jpg",
        "https://example.com/chocolate1-alt.jpg"
      ],
      "categoryId": 1,
      "category": {
        "id": 1,
        "name": "Chocolates Negros",
        "description": "Chocolates con alto contenido de cacao"
      },
      "producerId": 1,
      "producer": {
        "id": 1,
        "name": "Chocolatería Artesanal",
        "location": "Santander, Colombia"
      },
      "stock": 50,
      "isInStock": true,
      "featured": true,
      "rating": 4.5,
      "status": "Active",
      "isAvailableForPurchase": true,
      "createdAt": "2025-06-01T10:00:00Z",
      "updatedAt": "2025-06-15T14:30:00Z"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 12,
    "totalItems": 156,
    "totalPages": 13,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

#### **Endpoint**: `GET /api/v1/products/{id}`

**Descripción**: Muestra detalles completos de un producto específico.

**Ejemplo de request**:
```
GET /api/v1/products/1
```

**Respuesta exitosa (200)**:
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Chocolate Negro 70%",
    "slug": "chocolate-negro-70",
    "description": "Delicioso chocolate negro artesanal con 70% de cacao colombiano de origen único. Cultivado en las montañas de Santander por productores locales comprometidos con la calidad y sostenibilidad.",
    "price": 15000,
    "discountedPrice": 12000,
    "currentPrice": 12000,
    "hasDiscount": true,
    "image": "https://example.com/chocolate1.jpg",
    "images": [
      "https://example.com/chocolate1.jpg",
      "https://example.com/chocolate1-2.jpg",
      "https://example.com/chocolate1-3.jpg"
    ],
    "categoryId": 1,
    "category": {
      "id": 1,
      "name": "Chocolates Negros",
      "description": "Chocolates con alto contenido de cacao"
    },
    "producerId": 1,
    "producer": {
      "id": 1,
      "name": "Chocolatería Artesanal",
      "description": "Productores de chocolate orgánico",
      "location": "Santander, Colombia",
      "contactEmail": "contacto@chocolateriaartesanal.com"
    },
    "stock": 50,
    "isInStock": true,
    "featured": true,
    "rating": 4.5,
    "status": "Active",
    "isAvailableForPurchase": true,
    "createdAt": "2025-06-01T10:00:00Z",
    "updatedAt": "2025-06-15T14:30:00Z"
  }
}
```

**Errores posibles**:
- **404 Not Found**: Producto no encontrado

---

### **Etapa 4: Carrito de Compras** 🔒

> **Nota**: Todos los endpoints del carrito requieren autenticación.

#### **Endpoint**: `POST /api/v1/cart`

**Descripción**: Actualiza el carrito del usuario autenticado (añadir producto o modificar cantidad).

**Headers requeridos**:
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Body esperado**:
```json
{
  "id": 0,
  "userId": 1,
  "items": [
    {
      "productId": 1,
      "quantity": 2,
      "price": 12000
    }
  ],
  "total": 24000
}
```

**Respuesta exitosa (200)**:
```json
{
  "success": true,
  "message": "Cart updated successfully",
  "data": {
    "id": 1,
    "userId": 1,
    "items": [
      {
        "id": 1,
        "productId": 1,
        "productName": "Chocolate Negro 70%",
        "productImage": "https://example.com/chocolate1.jpg",
        "quantity": 2,
        "unitPrice": 12000,
        "totalPrice": 24000,
        "product": {
          "id": 1,
          "name": "Chocolate Negro 70%",
          "currentPrice": 12000,
          "image": "https://example.com/chocolate1.jpg",
          "stock": 48,
          "isInStock": true
        }
      }
    ],
    "total": 24000,
    "totalItems": 2,
    "createdAt": "2025-06-30T10:00:00Z",
    "updatedAt": "2025-06-30T10:30:00Z"
  }
}
```

#### **Endpoint**: `GET /api/v1/cart`

**Descripción**: Obtener el contenido actual del carrito del usuario autenticado.

**Headers requeridos**:
```
Authorization: Bearer {token}
```

**Respuesta exitosa (200)**:
```json
{
  "success": true,
  "message": "Cart retrieved successfully",
  "data": {
    "id": 1,
    "userId": 1,
    "items": [
      {
        "id": 1,
        "productId": 1,
        "productName": "Chocolate Negro 70%",
        "productImage": "https://example.com/chocolate1.jpg",
        "quantity": 2,
        "unitPrice": 12000,
        "totalPrice": 24000,
        "product": {
          "id": 1,
          "name": "Chocolate Negro 70%",
          "currentPrice": 12000,
          "image": "https://example.com/chocolate1.jpg",
          "stock": 48,
          "isInStock": true
        }
      }
    ],
    "total": 24000,
    "totalItems": 2,
    "createdAt": "2025-06-30T10:00:00Z",
    "updatedAt": "2025-06-30T10:30:00Z"
  }
}
```

#### **Endpoint**: `DELETE /api/v1/cart/items/{productId}`

**Descripción**: Eliminar un producto específico del carrito.

**Headers requeridos**:
```
Authorization: Bearer {token}
```

**Ejemplo de request**:
```
DELETE /api/v1/cart/items/1
```

**Respuesta exitosa (200)**:
```json
{
  "success": true,
  "message": "Product removed from cart successfully"
}
```

---

### **Etapa 5: Checkout y Orden** 🔒

#### **Endpoint**: `POST /api/v1/orders`

**Descripción**: Crear una orden a partir del carrito del usuario autenticado.

**Headers requeridos**:
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Body esperado**:
```json
{
  "paymentMethod": "CreditCard",
  "shippingAddress": {
    "name": "Juan Pérez",
    "address": "Calle 123 #45-67",
    "city": "Bogotá",
    "region": "Cundinamarca",
    "zipCode": "110111",
    "phone": "+57 300 123 4567"
  },
  "notes": "Entregar en portería del edificio"
}
```

**Respuesta exitosa (201)**:
```json
{
  "success": true,
  "message": "Order created successfully",
  "data": {
    "id": 1,
    "userId": 1,
    "status": "Pending",
    "total": 24000,
    "totalItems": 2,
    "paymentMethod": "CreditCard",
    "shippingAddress": {
      "name": "Juan Pérez",
      "address": "Calle 123 #45-67",
      "city": "Bogotá",
      "region": "Cundinamarca",
      "zipCode": "110111",
      "phone": "+57 300 123 4567"
    },
    "items": [
      {
        "id": 1,
        "productId": 1,
        "productName": "Chocolate Negro 70%",
        "quantity": 2,
        "unitPrice": 12000,
        "totalPrice": 24000
      }
    ],
    "notes": "Entregar en portería del edificio",
    "createdAt": "2025-06-30T10:30:00Z",
    "updatedAt": "2025-06-30T10:30:00Z"
  }
}
```

**Métodos de pago válidos**:
- `CreditCard`
- `DebitCard`
- `BankTransfer`
- `Cash`

#### **Endpoint**: `GET /api/v1/orders/{id}`

**Descripción**: Ver detalle de una orden específica.

**Headers requeridos**:
```
Authorization: Bearer {token}
```

**Ejemplo de request**:
```
GET /api/v1/orders/1
```

**Respuesta exitosa (200)**:
```json
{
  "success": true,
  "data": {
    "id": 1,
    "userId": 1,
    "status": "Processing",
    "total": 24000,
    "totalItems": 2,
    "paymentMethod": "CreditCard",
    "shippingAddress": {
      "name": "Juan Pérez",
      "address": "Calle 123 #45-67",
      "city": "Bogotá",
      "region": "Cundinamarca",
      "zipCode": "110111",
      "phone": "+57 300 123 4567"
    },
    "items": [
      {
        "id": 1,
        "productId": 1,
        "productName": "Chocolate Negro 70%",
        "quantity": 2,
        "unitPrice": 12000,
        "totalPrice": 24000,
        "product": {
          "id": 1,
          "name": "Chocolate Negro 70%",
          "slug": "chocolate-negro-70",
          "image": "https://example.com/chocolate1.jpg"
        }
      }
    ],
    "notes": "Entregar en portería del edificio",
    "createdAt": "2025-06-30T10:30:00Z",
    "updatedAt": "2025-06-30T11:00:00Z"
  }
}
```

**Estados de orden posibles**:
- `Pending`: Orden creada, pendiente de pago
- `Processing`: Orden en preparación
- `Shipped`: Orden enviada
- `Delivered`: Orden entregada
- `Cancelled`: Orden cancelada
- `Refunded`: Orden reembolsada

---

### **Etapa 6: Gestión del Perfil del Usuario** 🔒

#### **Endpoint**: `GET /api/v1/users/{id}`

**Descripción**: Obtener los datos del perfil del usuario autenticado.

**Headers requeridos**:
```
Authorization: Bearer {token}
```

**Ejemplo de request**:
```
GET /api/v1/users/1
```

**Respuesta exitosa (200)**:
```json
{
  "success": true,
  "message": "User profile retrieved successfully",
  "data": {
    "id": 1,
    "firstName": "Juan",
    "lastName": "Pérez",
    "email": "juan.perez@email.com",
    "phone": "+57 300 123 4567",
    "address": "Calle 123 #45-67, Bogotá",
    "role": "User",
    "isActive": true,
    "createdAt": "2025-06-01T10:00:00Z",
    "updatedAt": "2025-06-30T15:30:00Z"
  }
}
```

#### **Endpoint**: `PUT /api/v1/users/{id}`

**Descripción**: Actualizar los datos del perfil del usuario.

**Headers requeridos**:
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Body esperado**:
```json
{
  "firstName": "Juan Carlos",
  "lastName": "Pérez García",
  "phone": "+57 300 456 7890",
  "address": "Carrera 45 #123-67, Bogotá"
}
```

**Respuesta exitosa (200)**:
```json
{
  "success": true,
  "message": "User profile updated successfully",
  "data": {
    "id": 1,
    "firstName": "Juan Carlos",
    "lastName": "Pérez García",
    "email": "juan.perez@email.com",
    "phone": "+57 300 456 7890",
    "address": "Carrera 45 #123-67, Bogotá",
    "role": "User",
    "isActive": true,
    "createdAt": "2025-06-01T10:00:00Z",
    "updatedAt": "2025-06-30T16:00:00Z"
  }
}
```

---

## 📋 Notas Generales para el Frontend

### 🔐 Autenticación
- **Todas las rutas protegidas** (marcadas con 🔒) requieren enviar el token JWT en el header:
  ```
  Authorization: Bearer <token>
  ```
- El token tiene una **duración de 1 hora** (3600 segundos)
- Usar el **refresh token** para renovar el access token cuando expire
- Manejar respuestas **401 Unauthorized** redirigiendo al login

### 📨 Manejo de Respuestas
- **Todas las respuestas** siguen el formato estándar:
  ```json
  {
    "success": boolean,
    "message": "string",
    "data": object|array|null
  }
  ```
- **Códigos de estado HTTP estándar**:
  - `200`: Operación exitosa
  - `201`: Recurso creado exitosamente
  - `400`: Datos de entrada inválidos
  - `401`: No autenticado
  - `403`: Sin permisos
  - `404`: Recurso no encontrado
  - `409`: Conflicto (ej: email ya existe)
  - `500`: Error interno del servidor

### 🚨 Manejo de Errores
- **Las respuestas de error** deben manejarse de forma clara para mostrar mensajes amigables al usuario
- **Validar siempre** la propiedad `success` antes de procesar `data`
- **Mostrar mensajes contextuales** basados en el código de estado HTTP

### 🔄 Renovación de Token
```javascript
// Ejemplo de renovación de token
const renewToken = async (refreshToken, userId) => {
  const response = await fetch('/api/v1/auth/refresh-token', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken, userId })
  });
  
  if (response.ok) {
    const { data } = await response.json();
    localStorage.setItem('accessToken', data.accessToken);
    return data.accessToken;
  }
  
  // Redirigir al login si falla la renovación
  window.location.href = '/login';
};
```

### 📱 Paginación
- **Todos los endpoints con listados** soportan paginación
- **Parámetros estándar**:
  - `page`: Número de página (default: 1)
  - `pageSize`: Elementos por página (default: 10, max: 100)
- **Respuesta incluye** objeto `pagination` con metadatos

### 🛡️ Seguridad
- **Nunca almacenar** contraseñas en el frontend
- **Usar HTTPS** en producción
- **Almacenar tokens de forma segura** (localStorage/sessionStorage)
- **Limpiar tokens** al cerrar sesión

---

## � Funcionalidades de Administración Implementadas

### **Panel de Administración - Gestión de Productos** 🔒

#### **Endpoint**: `POST /api/v1/products`
**Descripción**: Crear un nuevo producto (solo administradores).

#### **Endpoint**: `PUT /api/v1/products/{id}`
**Descripción**: Actualizar un producto existente (solo administradores).

#### **Endpoint**: `DELETE /api/v1/products/{id}`
**Descripción**: Eliminar un producto (solo administradores).

### **Panel de Administración - Gestión de Órdenes** 🔒

#### **Endpoint**: `GET /api/v1/orders/user/{userId}`
**Descripción**: Obtener todas las órdenes de un usuario específico (solo administradores).

#### **Endpoint**: `PUT /api/v1/orders/{id}/status`
**Descripción**: Actualizar el estado de una orden (solo administradores).

**Body esperado**:
```json
{
  "status": "Processing"
}
```

**Estados válidos**: `Pending`, `Processing`, `Shipped`, `Delivered`, `Cancelled`, `Refunded`

### **Gestión de Inventario**

El sistema cuenta con un avanzado sistema de gestión de inventario que incluye:

#### **Reservas de Stock**
- **Endpoint**: `POST /api/v1/cart/reserve-stock` - Reservar stock del carrito
- **Endpoint**: `POST /api/v1/cart/release-reservations` - Liberar reservas
- **Endpoint**: `GET /api/v1/cart/validate-stock` - Validar disponibilidad

#### **Características del Sistema de Inventario**:
- ✅ **Reservas temporales**: Stock se reserva durante el proceso de checkout (15 minutos por defecto)
- ✅ **Limpieza automática**: Las reservas expiradas se limpian automáticamente cada 5 minutos
- ✅ **Validación en tiempo real**: Verificación de stock disponible considerando reservas activas
- ✅ **Confirmación de reservas**: Al completar la orden, las reservas se confirman y el stock se reduce definitivamente

### 🌟 Endpoints Adicionales Disponibles
- `GET /api/v1/categories`: Lista de categorías
- `GET /api/v1/producers`: Lista de productores
- `POST /api/v1/contact`: Formulario de contacto
- `POST /api/v1/newsletter`: Suscripción al newsletter
- `GET /api/v1/health`: Health check del sistema

---

## � Funcionalidades Faltantes por Implementar

### 📋 **Funcionalidades Críticas Faltantes**

#### **❤️ Sistema de Wishlist/Favoritos**
- `POST /api/v1/wishlist` - Agregar producto a favoritos
- `GET /api/v1/wishlist` - Obtener lista de favoritos del usuario
- `DELETE /api/v1/wishlist/{productId}` - Remover de favoritos
- **Impacto**: Alta retención de usuarios y mejora experiencia de compra

#### **⭐ Sistema de Reseñas y Calificaciones**
- `POST /api/v1/products/{id}/reviews` - Crear reseña
- `GET /api/v1/products/{id}/reviews` - Obtener reseñas de un producto
- `PUT /api/v1/reviews/{id}` - Actualizar reseña propia
- `DELETE /api/v1/reviews/{id}` - Eliminar reseña propia
- **Impacto**: Incrementa confianza y ayuda en decisión de compra

#### **� Dashboard de Administración Completo**
- `GET /api/v1/admin/dashboard/stats` - Estadísticas generales
- `GET /api/v1/admin/sales/reports` - Reportes de ventas
- `GET /api/v1/admin/products/analytics` - Analytics de productos
- `GET /api/v1/admin/users/stats` - Estadísticas de usuarios
- **Funcionalidades específicas**:
  - � Gráficos de ventas por período
  - 🏆 Productos más vendidos
  - 👥 Análisis de comportamiento de usuarios
  - 💰 Reportes financieros

#### **� Sistema de Descuentos y Cupones**
- `POST /api/v1/coupons` - Crear cupón (admin)
- `GET /api/v1/coupons/{code}/validate` - Validar cupón
- `POST /api/v1/cart/apply-coupon` - Aplicar cupón al carrito
- `DELETE /api/v1/cart/remove-coupon` - Remover cupón del carrito
- **Tipos de descuentos**: Porcentaje, monto fijo, envío gratis, primera compra

#### **📦 Sistema de Tracking de Envíos**
- `POST /api/v1/orders/{id}/tracking` - Agregar información de tracking
- `GET /api/v1/orders/{id}/tracking` - Obtener estado del envío
- `PUT /api/v1/orders/{id}/tracking` - Actualizar estado del envío
- **Estados de envío**: En preparación, Enviado, En tránsito, Entregado

#### **📧 Sistema de Notificaciones**
- `GET /api/v1/notifications` - Obtener notificaciones del usuario
- `PUT /api/v1/notifications/{id}/read` - Marcar como leída
- `POST /api/v1/notifications/preferences` - Configurar preferencias
- **Tipos**: Email, push notifications, SMS, notificaciones in-app

### 🔧 **Funcionalidades de Mejora**

#### **🔍 Búsqueda Avanzada**
- Filtros por rango de precios
- Filtros por calificaciones
- Filtros por disponibilidad
- Ordenamiento avanzado (popularidad, fecha, rating)

#### **📱 API para Aplicación Móvil**
- Endpoints optimizados para móvil
- Compresión de imágenes
- Paginación optimizada
- Caché agresivo

#### **💳 Pasarelas de Pago**
- Integración con Stripe
- Integración con PayU
- Integración con Mercado Pago
- Procesamiento seguro de pagos

#### **🌍 Funcionalidades Avanzadas**
- **Geolocalización**: Cálculo automático de costos de envío
- **Multi-idioma**: Soporte para español e inglés
- **Multi-moneda**: Soporte para COP, USD, EUR
- **Chat en vivo**: Sistema de soporte al cliente

---

## 📊 **Recomendación: Administración de Productos**

### **¿Desde dónde administrar los productos?**

**✅ RECOMENDACIÓN: Administración desde el Backend**

#### **Ventajas de administrar desde el backend:**
1. **🔐 Seguridad**: Control total de acceso y validaciones
2. **📊 Auditoría**: Registro completo de cambios y responsables  
3. **🔄 Sincronización**: Inventario siempre actualizado en tiempo real
4. **⚡ Performance**: Procesamiento optimizado en servidor
5. **📋 Validaciones**: Reglas de negocio centralizadas

#### **Flujo recomendado:**
1. **Panel de Admin Web**: Para gestión diaria por administradores
2. **API Backend**: Para operaciones programáticas y integraciones
3. **Frontend de Usuario**: Solo lectura y compras

#### **Endpoints ya implementados para administración:**
- ✅ `POST /api/v1/products` - Crear producto
- ✅ `PUT /api/v1/products/{id}` - Actualizar producto  
- ✅ `DELETE /api/v1/products/{id}` - Eliminar producto
- ✅ Control de stock automatizado
- ✅ Gestión de estados de órdenes
- ✅ Sistema de reservas de inventario

**El sistema actual ya tiene una base sólida para administración desde el backend. Solo falta implementar las funcionalidades adicionales listadas arriba.**

---

## 📞 Soporte

Para consultas sobre la integración, contactar al equipo de backend o revisar la documentación completa en Swagger UI: `https://localhost:5001/swagger`

---

*Documento actualizado: Junio 30, 2025*
*Versión del backend: .NET 9 / Clean Architecture*
