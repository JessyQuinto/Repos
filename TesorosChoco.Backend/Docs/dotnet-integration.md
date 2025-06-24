# Guía de Integración Frontend con .NET 9 Backend

## Configuración Inicial

### 1. Variables de Entorno
Copia el archivo `.env.local.example` como `.env.local`:
```bash
cp .env.local.example .env.local
```

Ajusta la URL del backend según tu configuración:
```bash
# Para desarrollo local
VITE_API_BASE_URL=https://localhost:7001

# Para API en otro puerto
VITE_API_BASE_URL=https://localhost:5001
```

### 2. Configuración del Backend .NET 9

Tu backend debe implementar los siguientes endpoints con la estructura esperada:

#### Autenticación (JWT)
```csharp
// POST /api/v1/auth/login
public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class AuthResponse
{
    public UserDto User { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public int RefreshTokenExpiresIn { get; set; }
}
```

#### CORS Configuration
```csharp
// En Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:8080", "https://localhost:8080")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

app.UseCors("FrontendPolicy");
```

#### API Versioning
```csharp
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ApiVersionReader = ApiVersionReader.Combine(
        new HeaderApiVersionReader("X-API-Version"),
        new QueryStringApiVersionReader("version")
    );
});
```

#### Problem Details (para manejo de errores)
```csharp
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = (context) =>
    {
        context.ProblemDetails.Instance = context.HttpContext.Request.Path;
        context.ProblemDetails.Extensions.TryAdd("traceId", context.HttpContext.TraceIdentifier);
    };
});
```

### 3. Estructura de Respuestas Esperadas

#### Respuesta Exitosa
```json
{
  "data": { /* contenido */ },
  "success": true,
  "message": "Operación exitosa",
  "metadata": {
    "timestamp": "2025-06-23T10:00:00Z",
    "correlationId": "abc123",
    "version": "1.0"
  }
}
```

#### Respuesta de Error (Problem Details)
```json
{
  "type": "https://tools.ietf.org/html/rfc7807",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "instance": "/api/v1/auth/login",
  "traceId": "abc123",
  "errors": {
    "Email": ["El email es requerido"],
    "Password": ["La contraseña debe tener al menos 6 caracteres"]
  }
}
```

### 4. Endpoints Requeridos

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/v1/auth/login` | Autenticación de usuario |
| POST | `/api/v1/auth/register` | Registro de usuario |
| POST | `/api/v1/auth/refresh-token` | Renovar token |
| POST | `/api/v1/auth/logout` | Cerrar sesión |
| GET | `/api/v1/auth/profile` | Perfil del usuario |
| GET | `/api/v1/products` | Lista de productos |
| GET | `/api/v1/products/featured` | Productos destacados |
| GET | `/api/v1/categories` | Categorías |
| POST | `/api/v1/cart` | Agregar al carrito |
| GET | `/api/v1/orders` | Órdenes del usuario |

### 5. Configuración de HTTPS en Desarrollo

Para desarrollo local con HTTPS, asegúrate de que tu API .NET 9 esté configurada con:

```csharp
// En launchSettings.json
{
  "profiles": {
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": false,
      "applicationUrl": "https://localhost:7001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

### 6. Testing de la Integración

Una vez configurado tu backend, puedes probar la integración:

1. **Iniciar el backend .NET 9**
2. **Iniciar el frontend**:
   ```bash
   npm run dev
   ```
3. **Verificar la conexión**:
   - Abrir Network tab en DevTools
   - Intentar login/registro
   - Verificar que las requests lleguen a la API

### 7. Solución de Problemas Comunes

#### Error de CORS
```
Access to fetch at 'https://localhost:7001/api/v1/auth/login' from origin 'http://localhost:8080' has been blocked by CORS policy
```
**Solución**: Verificar configuración CORS en el backend.

#### Error de certificado SSL
```
net::ERR_CERT_AUTHORITY_INVALID
```
**Solución**: Agregar `NODE_TLS_REJECT_UNAUTHORIZED=0` en `.env.local` solo para desarrollo.

#### Token no reconocido
```
401 Unauthorized
```
**Solución**: Verificar que el backend esté validando correctamente el JWT token.

### 8. Deployment

Para producción, actualizar las variables de entorno:
```bash
VITE_API_BASE_URL=https://tu-api.azurewebsites.net
VITE_API_SECURE=true
```
