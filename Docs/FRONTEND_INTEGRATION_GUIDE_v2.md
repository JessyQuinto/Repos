# 🍫 Documento de Integración Backend-Frontend para Proyecto E-commerce TesorosChoco

## 📋 Propósito

Este documento tiene como finalidad guiar la integración entre el backend y el frontend del sistema e-commerce TesorosChoco, priorizando el desarrollo backend. Se describen los servicios, rutas, controladores y otros elementos relevantes para que el equipo frontend pueda implementar correctamente cada funcionalidad siguiendo un flujo paso a paso.

## 🔧 Configuración Base

- **URL Base (Desarrollo Local)**: `https://localhost:5001` / `http://localhost:5000`
- **URL Base (Docker)**: `http://localhost:5002`
- **Versionado API**: `api/v1/`
- **Content-Type**: `application/json`
- **Autenticación**: JWT Bearer Token
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

## 🚀 Funcionalidades Futuras

Este documento puede expandirse para incluir:

### 📋 Próximas Etapas
- **❤️ Wishlist**: Productos favoritos
- **⭐ Reseñas de productos**: Sistema de calificaciones
- **🔧 Panel de administración**: Gestión de productos, órdenes y usuarios
- **📧 Notificaciones**: Email y push notifications
- **💳 Pasarelas de pago**: Integración con Stripe, PayU, etc.
- **📊 Reportes y estadísticas**: Dashboard analítico
- **🎯 Sistema de descuentos y cupones**
- **📦 Tracking de envíos**
- **💬 Chat de soporte**

### 🌟 Endpoints Adicionales Disponibles
- `GET /api/v1/categories`: Lista de categorías
- `GET /api/v1/producers`: Lista de productores
- `POST /api/v1/contact`: Formulario de contacto
- `POST /api/v1/newsletter`: Suscripción al newsletter
- `GET /api/v1/health`: Health check del sistema

---

## 📞 Soporte

Para consultas sobre la integración, contactar al equipo de backend o revisar la documentación completa en Swagger UI: `https://localhost:5001/swagger`

---

*Documento actualizado: Junio 30, 2025*
*Versión del backend: .NET 9 / Clean Architecture*
