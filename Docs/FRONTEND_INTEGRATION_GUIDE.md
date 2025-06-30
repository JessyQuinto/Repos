# 🍫 TesorosChoco Backend - Guía de Integración Frontend

## 📋 Información General

**TesorosChoco Backend** es una API REST para un e-commerce de chocolates artesanales desarrollada con **.NET 9** siguiendo principios de **Clean Architecture**.

### 🔧 Configuración Base
- **URL Base (Desarrollo)**: `http://localhost:5000` / `https://localhost:7001`
- **Versionado**: `api/v1/`
- **Content-Type**: `application/json`
- **Autenticación**: JWT Bearer Token

---

## 🔐 Autenticación

### Flujo de Autenticación
1. **Login** → Obtener Access Token + Refresh Token
2. **Incluir Bearer Token** en headers para endpoints protegidos
3. **Renovar Token** usando Refresh Token cuando expire

### Endpoints de Autenticación

#### `POST /api/v1/auth/login`
**Descripción**: Autentica un usuario y devuelve tokens de acceso.

**Request Body**:
```json
{
  "email": "usuario@ejemplo.com",
  "password": "password123"
}
```

**Response Success (200)**:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "user": {
      "id": 1,
      "firstName": "Juan",
      "lastName": "Pérez",
      "email": "usuario@ejemplo.com",
      "phone": "+57 300 123 4567",
      "address": "Calle 123 #45-67"
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_string",
    "tokenType": "Bearer",
    "expiresIn": 3600,
    "refreshTokenExpiresIn": 604800
  }
}
```

**Response Error (401)**:
```json
{
  "success": false,
  "message": "Invalid credentials"
}
```

#### `POST /api/v1/auth/register`
**Descripción**: Registra un nuevo usuario.

**Request Body**:
```json
{
  "firstName": "Juan",
  "lastName": "Pérez", 
  "email": "usuario@ejemplo.com",
  "password": "password123",
  "phone": "+57 300 123 4567",
  "address": "Calle 123 #45-67"
}
```

#### `POST /api/v1/auth/refresh-token`
**Descripción**: Renueva el access token usando el refresh token.

**Request Body**:
```json
{
  "refreshToken": "refresh_token_string",
  "userId": 1
}
```

#### `POST /api/v1/auth/forgot-password`
**Request Body**:
```json
{
  "email": "usuario@ejemplo.com"
}
```

#### `POST /api/v1/auth/reset-password`
**Request Body**:
```json
{
  "token": "reset_token",
  "email": "usuario@ejemplo.com",
  "newPassword": "newPassword123"
}
```

---

## 🛍️ Productos

### `GET /api/v1/products`
**Descripción**: Obtiene lista de productos con filtros opcionales.

**Query Parameters**:
- `featured` (bool, opcional): Productos destacados
- `categoryId` (int, opcional): Filtrar por categoría
- `producerId` (int, opcional): Filtrar por productor
- `searchTerm` (string, opcional): Búsqueda por nombre/descripción
- `page` (int, default: 1): Número de página
- `pageSize` (int, default: 10, max: 100): Elementos por página

**Ejemplo Request**:
```
GET /api/v1/products?featured=true&categoryId=1&page=1&pageSize=20
```

**Response**:
```json
[
  {
    "id": 1,
    "name": "Chocolate Negro 70%",
    "slug": "chocolate-negro-70",
    "description": "Delicioso chocolate negro artesanal...",
    "price": 15000,
    "discountedPrice": 12000,
    "image": "https://example.com/chocolate1.jpg",
    "images": [
      "https://example.com/chocolate1.jpg",
      "https://example.com/chocolate1-2.jpg"
    ],
    "categoryId": 1,
    "producerId": 1,
    "stock": 50,
    "featured": true,
    "rating": 4.5,
    "createdAt": "2025-06-01T10:00:00Z",
    "updatedAt": "2025-06-15T14:30:00Z"
  }
]
```

### `GET /api/v1/products/{id}`
**Descripción**: Obtiene un producto específico por ID.

**Response** (mismo formato que arriba, pero un solo objeto).

### `POST /api/v1/products` 🔒
**Descripción**: Crea un nuevo producto (Admin only).

**Headers**: `Authorization: Bearer {token}`

### `PUT /api/v1/products/{id}` 🔒
**Descripción**: Actualiza un producto (Admin only).

### `DELETE /api/v1/products/{id}` 🔒
**Descripción**: Elimina un producto (Admin only).

---

## 🛒 Carrito de Compras

### `GET /api/v1/cart` 🔒
**Descripción**: Obtiene el carrito del usuario autenticado.

**Headers**: `Authorization: Bearer {token}`

**Response**:
```json
{
  "success": true,
  "data": {
    "id": 1,
    "userId": 1,
    "items": [
      {
        "id": 1,
        "productId": 1,
        "quantity": 2,
        "price": 15000,
        "product": {
          "id": 1,
          "name": "Chocolate Negro 70%",
          "image": "https://example.com/chocolate1.jpg",
          "price": 15000,
          "discountedPrice": 12000
        }
      }
    ],
    "totalItems": 2,
    "totalAmount": 24000,
    "createdAt": "2025-06-30T10:00:00Z",
    "updatedAt": "2025-06-30T11:30:00Z"
  }
}
```

### `POST /api/v1/cart/items` 🔒
**Descripción**: Agrega un producto al carrito.

**Request Body**:
```json
{
  "productId": 1,
  "quantity": 2
}
```

### `PUT /api/v1/cart/items/{productId}` 🔒
**Descripción**: Actualiza la cantidad de un producto en el carrito.

**Request Body**:
```json
{
  "quantity": 3
}
```

### `DELETE /api/v1/cart/items/{productId}` 🔒
**Descripción**: Elimina un producto del carrito.

### `DELETE /api/v1/cart` 🔒
**Descripción**: Vacía el carrito completamente.

---

## 📦 Órdenes

### `GET /api/v1/orders` 🔒
**Descripción**: Obtiene las órdenes del usuario autenticado.

**Query Parameters**:
- `status` (string, opcional): Filtrar por estado (`Pending`, `Processing`, `Shipped`, `Delivered`, `Cancelled`)
- `page` (int): Paginación
- `pageSize` (int): Elementos por página

### `GET /api/v1/orders/{id}` 🔒
**Descripción**: Obtiene una orden específica.

**Response**:
```json
{
  "success": true,
  "data": {
    "id": 1,
    "userId": 1,
    "orderNumber": "ORD-2025-001",
    "status": "Processing",
    "totalAmount": 24000,
    "shippingCost": 5000,
    "finalAmount": 29000,
    "paymentMethod": "CreditCard",
    "paymentStatus": "Paid",
    "shippingAddress": {
      "street": "Calle 123 #45-67",
      "city": "Bogotá",
      "state": "Cundinamarca",
      "country": "Colombia",
      "postalCode": "110111"
    },
    "items": [
      {
        "productId": 1,
        "productName": "Chocolate Negro 70%",
        "quantity": 2,
        "unitPrice": 12000,
        "totalPrice": 24000
      }
    ],
    "createdAt": "2025-06-30T10:00:00Z",
    "updatedAt": "2025-06-30T11:00:00Z",
    "estimatedDeliveryDate": "2025-07-05T00:00:00Z"
  }
}
```

### `POST /api/v1/orders` 🔒
**Descripción**: Crea una nueva orden desde el carrito.

**Request Body**:
```json
{
  "paymentMethod": "CreditCard",
  "shippingAddress": {
    "street": "Calle 123 #45-67",
    "city": "Bogotá",
    "state": "Cundinamarca",
    "country": "Colombia",
    "postalCode": "110111",
    "recipientName": "Juan Pérez",
    "recipientPhone": "+57 300 123 4567"
  },
  "notes": "Entregar en portería"
}
```

### `PUT /api/v1/orders/{id}/cancel` 🔒
**Descripción**: Cancela una orden (solo si está en estado `Pending`).

---

## 📑 Categorías

### `GET /api/v1/categories`
**Descripción**: Obtiene todas las categorías de productos.

**Response**:
```json
[
  {
    "id": 1,
    "name": "Chocolates Negros",
    "description": "Chocolates con alto contenido de cacao",
    "image": "https://example.com/category1.jpg",
    "productCount": 15
  }
]
```

---

## 👥 Productores

### `GET /api/v1/producers`
**Descripción**: Obtiene todos los productores.

**Response**:
```json
[
  {
    "id": 1,
    "name": "Chocolatería Artesanal",
    "description": "Productores de chocolate orgánico",
    "location": "Santander, Colombia",
    "contactEmail": "contacto@chocolateriaartesanal.com",
    "website": "https://chocolateriaartesanal.com",
    "image": "https://example.com/producer1.jpg"
  }
]
```

---

## 📧 Newsletter

### `POST /api/v1/newsletter/subscribe`
**Descripción**: Suscribe un email al newsletter.

**Request Body**:
```json
{
  "email": "usuario@ejemplo.com",
  "firstName": "Juan"
}
```

### `POST /api/v1/newsletter/unsubscribe`
**Request Body**:
```json
{
  "email": "usuario@ejemplo.com"
}
```

---

## 📞 Contacto

### `POST /api/v1/contact`
**Descripción**: Envía un mensaje de contacto.

**Request Body**:
```json
{
  "name": "Juan Pérez",
  "email": "usuario@ejemplo.com",
  "subject": "Consulta sobre productos",
  "message": "Hola, me gustaría saber...",
  "phone": "+57 300 123 4567"
}
```

---

## ❤️ Estado del Sistema

### `GET /api/v1/health`
**Descripción**: Verifica el estado de la API y dependencias.

**Response**:
```json
{
  "status": "Healthy",
  "timestamp": "2025-06-30T15:30:00Z",
  "version": "1.0.0",
  "dependencies": {
    "database": "Healthy",
    "redis": "Healthy",
    "email": "Healthy"
  }
}
```

---

## 🔧 Configuración Frontend

### Headers Requeridos
```javascript
const headers = {
  'Content-Type': 'application/json',
  'Accept': 'application/json',
  // Para endpoints protegidos:
  'Authorization': `Bearer ${accessToken}`
};
```

### Ejemplo de Configuración Axios
```javascript
// config/api.js
import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5000/api/v1',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor para incluir token automáticamente
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Interceptor para manejar errores de token expirado
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      // Token expirado, intentar renovar
      const refreshToken = localStorage.getItem('refreshToken');
      if (refreshToken) {
        try {
          const response = await api.post('/auth/refresh-token', {
            refreshToken,
            userId: getCurrentUserId()
          });
          const { accessToken } = response.data.data;
          localStorage.setItem('accessToken', accessToken);
          // Reintentar request original
          error.config.headers.Authorization = `Bearer ${accessToken}`;
          return api.request(error.config);
        } catch {
          // Refresh falló, redirigir a login
          localStorage.clear();
          window.location.href = '/login';
        }
      }
    }
    return Promise.reject(error);
  }
);

export default api;
```

### Ejemplo de Uso en React
```javascript
// hooks/useAuth.js
import { useState, useEffect } from 'react';
import api from '../config/api';

export const useAuth = () => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  const login = async (email, password) => {
    try {
      const response = await api.post('/auth/login', { email, password });
      const { user, accessToken, refreshToken } = response.data.data;
      
      localStorage.setItem('accessToken', accessToken);
      localStorage.setItem('refreshToken', refreshToken);
      localStorage.setItem('user', JSON.stringify(user));
      
      setUser(user);
      return { success: true };
    } catch (error) {
      return { 
        success: false, 
        message: error.response?.data?.message || 'Error de login' 
      };
    }
  };

  const logout = () => {
    localStorage.clear();
    setUser(null);
  };

  useEffect(() => {
    const savedUser = localStorage.getItem('user');
    if (savedUser) {
      setUser(JSON.parse(savedUser));
    }
    setLoading(false);
  }, []);

  return { user, login, logout, loading };
};
```

---

## 🚨 Manejo de Errores

### Estructura de Respuesta de Error
```json
{
  "success": false,
  "message": "Descripción del error",
  "errors": ["Error específico 1", "Error específico 2"]
}
```

### Códigos de Estado HTTP
- **200**: Éxito
- **400**: Bad Request (datos inválidos)
- **401**: No autorizado (token inválido/expirado)
- **403**: Prohibido (sin permisos)
- **404**: No encontrado
- **500**: Error interno del servidor

### Ejemplo de Manejo de Errores
```javascript
// utils/errorHandler.js
export const handleApiError = (error) => {
  if (error.response) {
    const { status, data } = error.response;
    
    switch (status) {
      case 400:
        return data.message || 'Datos inválidos';
      case 401:
        localStorage.clear();
        window.location.href = '/login';
        return 'Sesión expirada';
      case 403:
        return 'No tienes permisos para esta acción';
      case 404:
        return 'Recurso no encontrado';
      case 500:
        return 'Error interno del servidor';
      default:
        return data.message || 'Error desconocido';
    }
  }
  return 'Error de conexión';
};
```

---

## 📱 Estados de Pedidos

Los pedidos pueden tener los siguientes estados:

| Estado | Descripción |
|--------|-------------|
| `Pending` | Orden creada, pendiente de pago |
| `Processing` | Pago confirmado, preparando envío |
| `Shipped` | Orden enviada |
| `Delivered` | Orden entregada |
| `Cancelled` | Orden cancelada |

---

## 🔒 Métodos de Pago Válidos

- `CreditCard`
- `DebitCard`
- `BankTransfer`
- `Cash`

> **⚠️ Nota Importante**: El backend actualmente **NO tiene integración real con pasarelas de pago**. Solo valida que el método sea válido, pero no procesa pagos reales. Esta funcionalidad debe implementarse próximamente.

---

## 🎯 Limitaciones Actuales

### ❌ **NO Implementado**
1. **Procesamiento real de pagos** (Stripe, PayPal, etc.)
2. **Sistema de cupones/descuentos**
3. **Notificaciones push/SMS**
4. **Sistema de wishlist**
5. **Recomendaciones de productos**
6. **Reviews/calificaciones de productos**
7. **Seguimiento de envíos**

### ✅ **Implementado y Funcional**
1. **Autenticación JWT completa**
2. **CRUD de productos con filtros**
3. **Gestión de carrito**
4. **Creación y gestión de órdenes**
5. **Gestión de stock con reservas**
6. **Sistema de categorías y productores**
7. **Newsletter y contacto**
8. **Notificaciones por email**
9. **Health checks**

---

## 🚀 Próximas Funcionalidades

1. **Integración con Stripe/PayPal**
2. **Sistema de cupones y promociones**
3. **API de tracking de envíos**
4. **Sistema de reviews**
5. **Notificaciones en tiempo real**
6. **Dashboard de administración**

---

## 📞 Soporte

Para dudas sobre la integración:
- **Email**: dev@tesoroschoco.com
- **Documentación**: [Swagger UI] `http://localhost:5000/swagger`
- **Repositorio**: TesorosChoco.Backend

---

**Última actualización**: 30 de Junio, 2025  
**Versión API**: v1.0.0
