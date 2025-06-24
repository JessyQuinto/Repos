
# API Specification - TesorosChocó E-commerce Platform

## Authentication Services

### 1. User Login
- **Método HTTP**: POST
- **Ruta**: `/api/auth/login`
- **Descripción**: Autentica a un usuario existente
- **JSON Request**:
```json
{
  "email": "string",
  "password": "string"
}
```
- **JSON Response**:
```json
{
  "user": {
    "id": "number",
    "firstName": "string",
    "lastName": "string",
    "email": "string",
    "phone": "string",
    "address": "string"
  },
  "token": "string"
}
```

### 2. User Registration
- **Método HTTP**: POST
- **Ruta**: `/api/auth/register`
- **Descripción**: Registra un nuevo usuario
- **JSON Request**:
```json
{
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "password": "string",
  "phone": "string",
  "address": "string"
}
```
- **JSON Response**:
```json
{
  "user": {
    "id": "number",
    "firstName": "string",
    "lastName": "string",
    "email": "string",
    "phone": "string",
    "address": "string"
  },
  "token": "string"
}
```

## Product Services

### 3. Get All Products
- **Método HTTP**: GET
- **Ruta**: `/api/products`
- **Descripción**: Obtiene la lista completa de productos
- **JSON Request**: N/A
- **JSON Response**:
```json
[
  {
    "id": "number",
    "name": "string",
    "slug": "string",
    "description": "string",
    "price": "number",
    "discountedPrice": "number|null",
    "image": "string",
    "images": "string[]",
    "categoryId": "number",
    "producerId": "number",
    "stock": "number",
    "featured": "boolean",
    "rating": "number|null",
    "createdAt": "string",
    "updatedAt": "string"
  }
]
```

### 4. Get Product by ID
- **Método HTTP**: GET
- **Ruta**: `/api/products/{id}`
- **Descripción**: Obtiene un producto específico por su ID
- **JSON Request**: N/A
- **JSON Response**:
```json
{
  "id": "number",
  "name": "string",
  "slug": "string",
  "description": "string",
  "price": "number",
  "discountedPrice": "number|null",
  "image": "string",
  "images": "string[]",
  "categoryId": "number",
  "producerId": "number",
  "stock": "number",
  "featured": "boolean",
  "rating": "number|null",
  "createdAt": "string",
  "updatedAt": "string"
}
```

### 5. Create Product
- **Método HTTP**: POST
- **Ruta**: `/api/products`
- **Descripción**: Crea un nuevo producto
- **JSON Request**:
```json
{
  "name": "string",
  "slug": "string",
  "description": "string",
  "price": "number",
  "discountedPrice": "number|null",
  "image": "string",
  "images": "string[]",
  "categoryId": "number",
  "producerId": "number",
  "stock": "number",
  "featured": "boolean"
}
```
- **JSON Response**:
```json
{
  "id": "number",
  "name": "string",
  "slug": "string",
  "description": "string",
  "price": "number",
  "discountedPrice": "number|null",
  "image": "string",
  "images": "string[]",
  "categoryId": "number",
  "producerId": "number",
  "stock": "number",
  "featured": "boolean",
  "rating": "number|null",
  "createdAt": "string",
  "updatedAt": "string"
}
```

### 6. Update Product
- **Método HTTP**: PUT
- **Ruta**: `/api/products/{id}`
- **Descripción**: Actualiza un producto existente
- **JSON Request**:
```json
{
  "id": "number",
  "name": "string",
  "slug": "string",
  "description": "string",
  "price": "number",
  "discountedPrice": "number|null",
  "image": "string",
  "images": "string[]",
  "categoryId": "number",
  "producerId": "number",
  "stock": "number",
  "featured": "boolean"
}
```
- **JSON Response**: N/A (204 No Content)

### 7. Delete Product
- **Método HTTP**: DELETE
- **Ruta**: `/api/products/{id}`
- **Descripción**: Elimina un producto
- **JSON Request**: N/A
- **JSON Response**: N/A (204 No Content)

## Category Services

### 8. Get All Categories
- **Método HTTP**: GET
- **Ruta**: `/api/categories`
- **Descripción**: Obtiene todas las categorías de productos
- **JSON Request**: N/A
- **JSON Response**:
```json
[
  {
    "id": "number",
    "name": "string",
    "slug": "string",
    "description": "string",
    "image": "string",
    "createdAt": "string",
    "updatedAt": "string"
  }
]
```

### 9. Get Category by ID
- **Método HTTP**: GET
- **Ruta**: `/api/categories/{id}`
- **Descripción**: Obtiene una categoría específica por su ID
- **JSON Request**: N/A
- **JSON Response**:
```json
{
  "id": "number",
  "name": "string",
  "slug": "string",
  "description": "string",
  "image": "string",
  "createdAt": "string",
  "updatedAt": "string"
}
```

## Producer Services

### 10. Get All Producers
- **Método HTTP**: GET
- **Ruta**: `/api/producers`
- **Descripción**: Obtiene todos los productores/artesanos
- **JSON Request**: N/A
- **JSON Response**:
```json
[
  {
    "id": "number",
    "name": "string",
    "description": "string",
    "location": "string",
    "image": "string",
    "featured": "boolean",
    "foundedYear": "number",
    "createdAt": "string",
    "updatedAt": "string"
  }
]
```

### 11. Get Producer by ID
- **Método HTTP**: GET
- **Ruta**: `/api/producers/{id}`
- **Descripción**: Obtiene un productor específico por su ID
- **JSON Request**: N/A
- **JSON Response**:
```json
{
  "id": "number",
  "name": "string",
  "description": "string",
  "location": "string",
  "image": "string",
  "featured": "boolean",
  "foundedYear": "number",
  "createdAt": "string",
  "updatedAt": "string"
}
```

## Cart Services

### 12. Get User Cart
- **Método HTTP**: GET
- **Ruta**: `/api/cart`
- **Descripción**: Obtiene el carrito del usuario autenticado
- **JSON Request**: N/A
- **JSON Response**:
```json
{
  "id": "number",
  "userId": "number",
  "items": [
    {
      "productId": "number",
      "quantity": "number",
      "price": "number"
    }
  ],
  "total": "number",
  "createdAt": "string",
  "updatedAt": "string"
}
```

### 13. Update Cart
- **Método HTTP**: POST
- **Ruta**: `/api/cart`
- **Descripción**: Actualiza el carrito del usuario (agregar/modificar items)
- **JSON Request**:
```json
{
  "id": "number",
  "userId": "number",
  "items": [
    {
      "productId": "number",
      "quantity": "number",
      "price": "number"
    }
  ],
  "total": "number"
}
```
- **JSON Response**:
```json
{
  "id": "number",
  "userId": "number",
  "items": [
    {
      "productId": "number",
      "quantity": "number",
      "price": "number"
    }
  ],
  "total": "number",
  "createdAt": "string",
  "updatedAt": "string"
}
```

### 14. Clear Cart
- **Método HTTP**: DELETE
- **Ruta**: `/api/cart`
- **Descripción**: Vacía completamente el carrito del usuario
- **JSON Request**: N/A
- **JSON Response**: N/A (204 No Content)

## Order Services

### 15. Create Order
- **Método HTTP**: POST
- **Ruta**: `/api/orders`
- **Descripción**: Crea una nueva orden de compra
- **JSON Request**:
```json
{
  "userId": "number",
  "items": [
    {
      "productId": "number",
      "quantity": "number",
      "price": "number"
    }
  ],
  "shippingAddress": {
    "name": "string",
    "address": "string",
    "city": "string",
    "zipCode": "string",
    "phone": "string"
  },
  "paymentMethod": "string",
  "total": "number"
}
```
- **JSON Response**:
```json
{
  "id": "number",
  "userId": "number",
  "status": "string",
  "items": [
    {
      "productId": "number",
      "quantity": "number",
      "price": "number"
    }
  ],
  "shippingAddress": {
    "name": "string",
    "address": "string",
    "city": "string",
    "zipCode": "string",
    "phone": "string"
  },
  "paymentMethod": "string",
  "total": "number",
  "createdAt": "string",
  "updatedAt": "string"
}
```

### 16. Get Order by ID
- **Método HTTP**: GET
- **Ruta**: `/api/orders/{id}`
- **Descripción**: Obtiene una orden específica por su ID
- **JSON Request**: N/A
- **JSON Response**:
```json
{
  "id": "number",
  "userId": "number",
  "status": "string",
  "items": [
    {
      "productId": "number",
      "quantity": "number",
      "price": "number"
    }
  ],
  "shippingAddress": {
    "name": "string",
    "address": "string",
    "city": "string",
    "zipCode": "string",
    "phone": "string"
  },
  "paymentMethod": "string",
  "total": "number",
  "createdAt": "string",
  "updatedAt": "string"
}
```

### 17. Get User Orders
- **Método HTTP**: GET
- **Ruta**: `/api/orders/user/{userId}`
- **Descripción**: Obtiene todas las órdenes de un usuario específico
- **JSON Request**: N/A
- **JSON Response**:
```json
[
  {
    "id": "number",
    "userId": "number",
    "status": "string",
    "items": [
      {
        "productId": "number",
        "quantity": "number",
        "price": "number"
      }
    ],
    "shippingAddress": {
      "name": "string",
      "address": "string",
      "city": "string",
      "zipCode": "string",
      "phone": "string"
    },
    "paymentMethod": "string",
    "total": "number",
    "createdAt": "string",
    "updatedAt": "string"
  }
]
```

### 18. Update Order Status
- **Método HTTP**: PATCH
- **Ruta**: `/api/orders/{id}/status`
- **Descripción**: Actualiza el estado de una orden
- **JSON Request**:
```json
{
  "status": "string"
}
```
- **JSON Response**:
```json
{
  "id": "number",
  "status": "string",
  "updatedAt": "string"
}
```

## User Services

### 19. Get User Profile
- **Método HTTP**: GET
- **Ruta**: `/api/users/{id}`
- **Descripción**: Obtiene el perfil de un usuario específico
- **JSON Request**: N/A
- **JSON Response**:
```json
{
  "id": "number",
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "phone": "string",
  "address": "string",
  "createdAt": "string",
  "updatedAt": "string"
}
```

### 20. Update User Profile
- **Método HTTP**: PUT
- **Ruta**: `/api/users/{id}`
- **Descripción**: Actualiza el perfil de un usuario
- **JSON Request**:
```json
{
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "phone": "string",
  "address": "string"
}
```
- **JSON Response**:
```json
{
  "id": "number",
  "firstName": "string",
  "lastName": "string",
  "email": "string",
  "phone": "string",
  "address": "string",
  "updatedAt": "string"
}
```

## Search and Filter Services

### 21. Search Products
- **Método HTTP**: GET
- **Ruta**: `/api/products/search`
- **Query Parameters**: `?q={searchTerm}&category={categoryId}&minPrice={number}&maxPrice={number}&producer={producerId}&featured={boolean}&limit={number}&offset={number}`
- **Descripción**: Busca productos con filtros específicos
- **JSON Request**: N/A
- **JSON Response**:
```json
{
  "products": [
    {
      "id": "number",
      "name": "string",
      "slug": "string",
      "description": "string",
      "price": "number",
      "discountedPrice": "number|null",
      "image": "string",
      "images": "string[]",
      "categoryId": "number",
      "producerId": "number",
      "stock": "number",
      "featured": "boolean",
      "rating": "number|null",
      "createdAt": "string",
      "updatedAt": "string"
    }
  ],
  "total": "number",
  "page": "number",
  "limit": "number"
}
```

### 22. Get Featured Products
- **Método HTTP**: GET
- **Ruta**: `/api/products/featured`
- **Descripción**: Obtiene productos destacados
- **JSON Request**: N/A
- **JSON Response**:
```json
[
  {
    "id": "number",
    "name": "string",
    "slug": "string",
    "description": "string",
    "price": "number",
    "discountedPrice": "number|null",
    "image": "string",
    "images": "string[]",
    "categoryId": "number",
    "producerId": "number",
    "stock": "number",
    "featured": "boolean",
    "rating": "number|null",
    "createdAt": "string",
    "updatedAt": "string"
  }
]
```

### 23. Get Products by Category
- **Método HTTP**: GET
- **Ruta**: `/api/categories/{categoryId}/products`
- **Descripción**: Obtiene productos de una categoría específica
- **JSON Request**: N/A
- **JSON Response**:
```json
[
  {
    "id": "number",
    "name": "string",
    "slug": "string",
    "description": "string",
    "price": "number",
    "discountedPrice": "number|null",
    "image": "string",
    "images": "string[]",
    "categoryId": "number",
    "producerId": "number",
    "stock": "number",
    "featured": "boolean",
    "rating": "number|null",
    "createdAt": "string",
    "updatedAt": "string"
  }
]
```

### 24. Get Products by Producer
- **Método HTTP**: GET
- **Ruta**: `/api/producers/{producerId}/products`
- **Descripción**: Obtiene productos de un productor específico
- **JSON Request**: N/A
- **JSON Response**:
```json
[
  {
    "id": "number",
    "name": "string",
    "slug": "string",
    "description": "string",
    "price": "number",
    "discountedPrice": "number|null",
    "image": "string",
    "images": "string[]",
    "categoryId": "number",
    "producerId": "number",
    "stock": "number",
    "featured": "boolean",
    "rating": "number|null",
    "createdAt": "string",
    "updatedAt": "string"
  }
]
```

## Additional Services

### 25. Contact Form Submission
- **Método HTTP**: POST
- **Ruta**: `/api/contact`
- **Descripción**: Envía un mensaje de contacto
- **JSON Request**:
```json
{
  "name": "string",
  "email": "string",
  "subject": "string",
  "message": "string"
}
```
- **JSON Response**:
```json
{
  "success": "boolean",
  "message": "string"
}
```

### 26. Newsletter Subscription
- **Método HTTP**: POST
- **Ruta**: `/api/newsletter/subscribe`
- **Descripción**: Suscribe un email al newsletter
- **JSON Request**:
```json
{
  "email": "string"
}
```
- **JSON Response**:
```json
{
  "success": "boolean",
  "message": "string"
}
```

## Notes

### Authentication
- Todos los endpoints que requieren autenticación deben incluir el header: `Authorization: Bearer {token}`
- Los endpoints protegidos son: carrito, órdenes, perfil de usuario

### Error Responses
- Todos los endpoints pueden devolver errores con estructura:
```json
{
  "error": "string",
  "message": "string",
  "statusCode": "number"
}
```

### Status Codes
- 200: Éxito
- 201: Creado exitosamente
- 204: Sin contenido (eliminación exitosa)
- 400: Solicitud incorrecta
- 401: No autorizado
- 403: Prohibido
- 404: No encontrado
- 500: Error interno del servidor
