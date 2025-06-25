# Guía de Integración Frontend - TesorosChocó API

## Tabla de Contenidos
1. [Introducción](#introducción)
2. [Configuración Inicial](#configuración-inicial)
3. [Autenticación](#autenticación)
4. [Arquitectura del Cliente HTTP](#arquitectura-del-cliente-http)
5. [Servicios por Controlador](#servicios-por-controlador)
6. [Manejo de Errores](#manejo-de-errores)
7. [Seguridad](#seguridad)
8. [Buenas Prácticas](#buenas-prácticas)
9. [Ejemplos Completos](#ejemplos-completos)

## Introducción

Esta guía está diseñada para desarrolladores frontend que necesitan integrar sus aplicaciones con la API de TesorosChocó. La API utiliza .NET 9 con arquitectura limpia y está documentada para facilitar el consumo desde cualquier framework frontend (React, Vue, Angular, etc.).

### Características de la API
- Base URL: `http://localhost:5000/api/v1`
- Formato de respuesta: JSON con estructura `ApiResponse<T>`
- Autenticación: JWT Bearer Token
- Versionado: API v1
- CORS habilitado para desarrollo

## Configuración Inicial

### Instalación de Dependencias

#### Para JavaScript/TypeScript con Axios (Recomendado)
```bash
npm install axios
npm install @types/axios  # Si usas TypeScript
```

#### Para Fetch API Nativo
No requiere instalación adicional, disponible en todos los navegadores modernos.

### Configuración Base

```typescript
// config/api.ts
export const API_CONFIG = {
  BASE_URL: 'http://localhost:5000/api/v1',
  TIMEOUT: 10000,
  HEADERS: {
    'Content-Type': 'application/json',
    'Accept': 'application/json'
  }
};

// Tipos base para las respuestas de la API
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message: string;
  metadata: ApiMetadata;
}

export interface ApiMetadata {
  timestamp: string;
  version: string;
  requestId: string;
}
```

## Autenticación

### Tipos de Autenticación

```typescript
// types/auth.ts
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phone: string;
  address: string;
}

export interface AuthResponse {
  user: UserDto;
  accessToken: string;
  refreshToken: string;
  tokenType: string;
  expiresIn: number;
  refreshTokenExpiresIn: number;
}

export interface UserDto {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  address: string;
  createdAt: string;
  updatedAt: string;
}
```

### Gestión de Tokens

```typescript
// utils/tokenManager.ts
class TokenManager {
  private static instance: TokenManager;
  private accessToken: string | null = null;
  private refreshToken: string | null = null;

  private constructor() {
    this.loadTokensFromStorage();
  }

  static getInstance(): TokenManager {
    if (!TokenManager.instance) {
      TokenManager.instance = new TokenManager();
    }
    return TokenManager.instance;
  }

  setTokens(accessToken: string, refreshToken: string): void {
    this.accessToken = accessToken;
    this.refreshToken = refreshToken;
    
    localStorage.setItem('access_token', accessToken);
    localStorage.setItem('refresh_token', refreshToken);
  }

  getAccessToken(): string | null {
    return this.accessToken;
  }

  getRefreshToken(): string | null {
    return this.refreshToken;
  }

  clearTokens(): void {
    this.accessToken = null;
    this.refreshToken = null;
    
    localStorage.removeItem('access_token');
    localStorage.removeItem('refresh_token');
  }

  isAuthenticated(): boolean {
    return !!this.accessToken;
  }

  private loadTokensFromStorage(): void {
    this.accessToken = localStorage.getItem('access_token');
    this.refreshToken = localStorage.getItem('refresh_token');
  }
}

export default TokenManager;
```

## Arquitectura del Cliente HTTP

### Cliente HTTP Base con Axios

```typescript
// services/httpClient.ts
import axios, { AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios';
import { API_CONFIG } from '../config/api';
import TokenManager from '../utils/tokenManager';

class HttpClient {
  private static instance: HttpClient;
  private axiosInstance: AxiosInstance;
  private tokenManager: TokenManager;

  private constructor() {
    this.tokenManager = TokenManager.getInstance();
    this.axiosInstance = axios.create({
      baseURL: API_CONFIG.BASE_URL,
      timeout: API_CONFIG.TIMEOUT,
      headers: API_CONFIG.HEADERS
    });

    this.setupInterceptors();
  }

  static getInstance(): HttpClient {
    if (!HttpClient.instance) {
      HttpClient.instance = new HttpClient();
    }
    return HttpClient.instance;
  }

  private setupInterceptors(): void {
    // Request interceptor para agregar token
    this.axiosInstance.interceptors.request.use(
      (config) => {
        const token = this.tokenManager.getAccessToken();
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Response interceptor para manejo de errores
    this.axiosInstance.interceptors.response.use(
      (response) => response,
      async (error) => {
        const originalRequest = error.config;

        if (error.response?.status === 401 && !originalRequest._retry) {
          originalRequest._retry = true;
          
          try {
            await this.refreshTokens();
            const token = this.tokenManager.getAccessToken();
            originalRequest.headers.Authorization = `Bearer ${token}`;
            return this.axiosInstance(originalRequest);
          } catch (refreshError) {
            this.tokenManager.clearTokens();
            // Redirigir al login
            window.location.href = '/login';
            return Promise.reject(refreshError);
          }
        }

        return Promise.reject(error);
      }
    );
  }

  private async refreshTokens(): Promise<void> {
    const refreshToken = this.tokenManager.getRefreshToken();
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    const response = await axios.post(`${API_CONFIG.BASE_URL}/auth/refresh`, {
      refreshToken
    });

    const { accessToken, refreshToken: newRefreshToken } = response.data.data;
    this.tokenManager.setTokens(accessToken, newRefreshToken);
  }

  async get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.axiosInstance.get<ApiResponse<T>>(url, config);
    return response.data.data;
  }

  async post<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.axiosInstance.post<ApiResponse<T>>(url, data, config);
    return response.data.data;
  }

  async put<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.axiosInstance.put<ApiResponse<T>>(url, data, config);
    return response.data.data;
  }

  async delete<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.axiosInstance.delete<ApiResponse<T>>(url, config);
    return response.data.data;
  }

  async patch<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<T> {
    const response = await this.axiosInstance.patch<ApiResponse<T>>(url, data, config);
    return response.data.data;
  }
}

export default HttpClient;
```

### Cliente HTTP Alternativo con Fetch

```typescript
// services/fetchClient.ts
import { API_CONFIG } from '../config/api';
import TokenManager from '../utils/tokenManager';

class FetchClient {
  private static instance: FetchClient;
  private tokenManager: TokenManager;

  private constructor() {
    this.tokenManager = TokenManager.getInstance();
  }

  static getInstance(): FetchClient {
    if (!FetchClient.instance) {
      FetchClient.instance = new FetchClient();
    }
    return FetchClient.instance;
  }

  private async makeRequest<T>(
    url: string, 
    options: RequestInit = {}
  ): Promise<T> {
    const token = this.tokenManager.getAccessToken();
    
    const config: RequestInit = {
      ...options,
      headers: {
        ...API_CONFIG.HEADERS,
        ...(token && { Authorization: `Bearer ${token}` }),
        ...options.headers
      }
    };

    const response = await fetch(`${API_CONFIG.BASE_URL}${url}`, config);

    if (response.status === 401) {
      try {
        await this.refreshTokens();
        // Retry with new token
        const newToken = this.tokenManager.getAccessToken();
        config.headers = {
          ...config.headers,
          Authorization: `Bearer ${newToken}`
        };
        const retryResponse = await fetch(`${API_CONFIG.BASE_URL}${url}`, config);
        if (!retryResponse.ok) {
          throw new Error(`HTTP error! status: ${retryResponse.status}`);
        }
        const retryData = await retryResponse.json();
        return retryData.data;
      } catch (error) {
        this.tokenManager.clearTokens();
        window.location.href = '/login';
        throw error;
      }
    }

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || `HTTP error! status: ${response.status}`);
    }

    const data = await response.json();
    return data.data;
  }

  private async refreshTokens(): Promise<void> {
    const refreshToken = this.tokenManager.getRefreshToken();
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    const response = await fetch(`${API_CONFIG.BASE_URL}/auth/refresh`, {
      method: 'POST',
      headers: API_CONFIG.HEADERS,
      body: JSON.stringify({ refreshToken })
    });

    if (!response.ok) {
      throw new Error('Failed to refresh token');
    }

    const data = await response.json();
    const { accessToken, refreshToken: newRefreshToken } = data.data;
    this.tokenManager.setTokens(accessToken, newRefreshToken);
  }

  async get<T>(url: string): Promise<T> {
    return this.makeRequest<T>(url, { method: 'GET' });
  }

  async post<T>(url: string, data?: any): Promise<T> {
    return this.makeRequest<T>(url, {
      method: 'POST',
      body: data ? JSON.stringify(data) : undefined
    });
  }

  async put<T>(url: string, data?: any): Promise<T> {
    return this.makeRequest<T>(url, {
      method: 'PUT',
      body: data ? JSON.stringify(data) : undefined
    });
  }

  async delete<T>(url: string): Promise<T> {
    return this.makeRequest<T>(url, { method: 'DELETE' });
  }
}

export default FetchClient;
```

## Servicios por Controlador

### Servicio de Autenticación

```typescript
// services/authService.ts
import HttpClient from './httpClient';
import TokenManager from '../utils/tokenManager';
import { LoginRequest, RegisterRequest, AuthResponse } from '../types/auth';

class AuthService {
  private httpClient: HttpClient;
  private tokenManager: TokenManager;

  constructor() {
    this.httpClient = HttpClient.getInstance();
    this.tokenManager = TokenManager.getInstance();
  }

  async login(credentials: LoginRequest): Promise<AuthResponse> {
    try {
      const response = await this.httpClient.post<AuthResponse>('/auth/login', credentials);
      
      // Guardar tokens
      this.tokenManager.setTokens(response.accessToken, response.refreshToken);
      
      return response;
    } catch (error) {
      console.error('Login failed:', error);
      throw new Error('Credenciales inválidas');
    }
  }

  async register(userData: RegisterRequest): Promise<AuthResponse> {
    try {
      const response = await this.httpClient.post<AuthResponse>('/auth/register', userData);
      
      // Guardar tokens
      this.tokenManager.setTokens(response.accessToken, response.refreshToken);
      
      return response;
    } catch (error) {
      console.error('Registration failed:', error);
      throw new Error('Error en el registro');
    }
  }

  async logout(): Promise<void> {
    try {
      await this.httpClient.post('/auth/logout');
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      this.tokenManager.clearTokens();
    }
  }

  async refreshToken(): Promise<AuthResponse> {
    const refreshToken = this.tokenManager.getRefreshToken();
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    const response = await this.httpClient.post<AuthResponse>('/auth/refresh', {
      refreshToken
    });

    this.tokenManager.setTokens(response.accessToken, response.refreshToken);
    return response;
  }

  isAuthenticated(): boolean {
    return this.tokenManager.isAuthenticated();
  }
}

export default new AuthService();
```

### Servicio de Productos

```typescript
// types/product.ts
export interface ProductDto {
  id: number;
  name: string;
  slug: string;
  description: string;
  price: number;
  discountedPrice?: number;
  image: string;
  images: string[];
  categoryId: number;
  producerId: number;
  stock: number;
  featured: boolean;
  rating?: number;
  createdAt: string;
  updatedAt: string;
}

export interface ProductFilters {
  featured?: boolean;
  categoryId?: number;
  producerId?: number;
  searchTerm?: string;
  page?: number;
  pageSize?: number;
}

// services/productService.ts
import HttpClient from './httpClient';
import { ProductDto, ProductFilters } from '../types/product';

class ProductService {
  private httpClient: HttpClient;

  constructor() {
    this.httpClient = HttpClient.getInstance();
  }

  async getProducts(filters: ProductFilters = {}): Promise<ProductDto[]> {
    const queryParams = new URLSearchParams();
    
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        queryParams.append(key, value.toString());
      }
    });

    const queryString = queryParams.toString();
    const url = queryString ? `/products?${queryString}` : '/products';
    
    return this.httpClient.get<ProductDto[]>(url);
  }

  async getProductById(id: number): Promise<ProductDto> {
    return this.httpClient.get<ProductDto>(`/products/${id}`);
  }

  async getFeaturedProducts(): Promise<ProductDto[]> {
    return this.getProducts({ featured: true });
  }

  async searchProducts(searchTerm: string, page = 1, pageSize = 10): Promise<ProductDto[]> {
    return this.getProducts({ searchTerm, page, pageSize });
  }

  async getProductsByCategory(categoryId: number): Promise<ProductDto[]> {
    return this.getProducts({ categoryId });
  }

  async getProductsByProducer(producerId: number): Promise<ProductDto[]> {
    return this.getProducts({ producerId });
  }
}

export default new ProductService();
```

### Servicio de Carrito

```typescript
// types/cart.ts
export interface CartItemDto {
  id: number;
  productId: number;
  quantity: number;
  price: number;
  product: ProductDto;
}

export interface CartDto {
  id: number;
  userId: number;
  items: CartItemDto[];
  totalAmount: number;
  createdAt: string;
  updatedAt: string;
}

export interface UpdateCartRequest {
  items: Array<{
    productId: number;
    quantity: number;
  }>;
}

// services/cartService.ts
import HttpClient from './httpClient';
import { CartDto, UpdateCartRequest } from '../types/cart';

class CartService {
  private httpClient: HttpClient;

  constructor() {
    this.httpClient = HttpClient.getInstance();
  }

  async getCart(): Promise<CartDto> {
    return this.httpClient.get<CartDto>('/cart');
  }

  async updateCart(cartData: UpdateCartRequest): Promise<CartDto> {
    return this.httpClient.post<CartDto>('/cart', cartData);
  }

  async addToCart(productId: number, quantity: number): Promise<CartDto> {
    // Primero obtenemos el carrito actual
    const currentCart = await this.getCart();
    
    // Buscamos si el producto ya existe
    const existingItem = currentCart.items.find(item => item.productId === productId);
    
    let updatedItems;
    if (existingItem) {
      // Si existe, actualizamos la cantidad
      updatedItems = currentCart.items.map(item => 
        item.productId === productId 
          ? { productId: item.productId, quantity: item.quantity + quantity }
          : { productId: item.productId, quantity: item.quantity }
      );
    } else {
      // Si no existe, lo agregamos
      updatedItems = [
        ...currentCart.items.map(item => ({ 
          productId: item.productId, 
          quantity: item.quantity 
        })),
        { productId, quantity }
      ];
    }

    return this.updateCart({ items: updatedItems });
  }

  async removeFromCart(productId: number): Promise<CartDto> {
    const currentCart = await this.getCart();
    const updatedItems = currentCart.items
      .filter(item => item.productId !== productId)
      .map(item => ({ productId: item.productId, quantity: item.quantity }));

    return this.updateCart({ items: updatedItems });
  }

  async updateQuantity(productId: number, quantity: number): Promise<CartDto> {
    if (quantity <= 0) {
      return this.removeFromCart(productId);
    }

    const currentCart = await this.getCart();
    const updatedItems = currentCart.items.map(item => 
      item.productId === productId 
        ? { productId: item.productId, quantity }
        : { productId: item.productId, quantity: item.quantity }
    );

    return this.updateCart({ items: updatedItems });
  }

  async clearCart(): Promise<void> {
    await this.httpClient.delete<void>('/cart');
  }
}

export default new CartService();
```

### Servicio de Categorías

```typescript
// types/category.ts
export interface CategoryDto {
  id: number;
  name: string;
  description: string;
  slug: string;
  image?: string;
  createdAt: string;
  updatedAt: string;
}

// services/categoryService.ts
import HttpClient from './httpClient';
import { CategoryDto } from '../types/category';

class CategoryService {
  private httpClient: HttpClient;

  constructor() {
    this.httpClient = HttpClient.getInstance();
  }

  async getCategories(): Promise<CategoryDto[]> {
    return this.httpClient.get<CategoryDto[]>('/categories');
  }

  async getCategoryById(id: number): Promise<CategoryDto> {
    return this.httpClient.get<CategoryDto>(`/categories/${id}`);
  }

  async getCategoryBySlug(slug: string): Promise<CategoryDto> {
    return this.httpClient.get<CategoryDto>(`/categories/slug/${slug}`);
  }
}

export default new CategoryService();
```

### Servicio de Órdenes

```typescript
// types/order.ts
export interface OrderItemDto {
  id: number;
  productId: number;
  quantity: number;
  price: number;
  product: ProductDto;
}

export interface OrderDto {
  id: number;
  userId: number;
  status: string;
  totalAmount: number;
  shippingAddress: string;
  items: OrderItemDto[];
  createdAt: string;
  updatedAt: string;
}

export interface CreateOrderRequest {
  shippingAddress: string;
  items: Array<{
    productId: number;
    quantity: number;
  }>;
}

// services/orderService.ts
import HttpClient from './httpClient';
import { OrderDto, CreateOrderRequest } from '../types/order';

class OrderService {
  private httpClient: HttpClient;

  constructor() {
    this.httpClient = HttpClient.getInstance();
  }

  async createOrder(orderData: CreateOrderRequest): Promise<OrderDto> {
    return this.httpClient.post<OrderDto>('/orders', orderData);
  }

  async getOrders(): Promise<OrderDto[]> {
    return this.httpClient.get<OrderDto[]>('/orders');
  }

  async getOrderById(id: number): Promise<OrderDto> {
    return this.httpClient.get<OrderDto>(`/orders/${id}`);
  }

  async cancelOrder(id: number): Promise<OrderDto> {
    return this.httpClient.patch<OrderDto>(`/orders/${id}/cancel`);
  }
}

export default new OrderService();
```

## Manejo de Errores

### Clase de Error Personalizada

```typescript
// utils/apiError.ts
export class ApiError extends Error {
  public status: number;
  public data?: any;

  constructor(message: string, status: number, data?: any) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.data = data;
  }
}

// utils/errorHandler.ts
import { ApiError } from './apiError';

export class ErrorHandler {
  static handle(error: any): string {
    if (error instanceof ApiError) {
      switch (error.status) {
        case 400:
          return error.data?.message || 'Datos inválidos';
        case 401:
          return 'No autorizado. Por favor, inicia sesión';
        case 403:
          return 'No tienes permisos para realizar esta acción';
        case 404:
          return 'Recurso no encontrado';
        case 500:
          return 'Error interno del servidor';
        default:
          return error.message || 'Error desconocido';
      }
    }

    if (error.code === 'NETWORK_ERROR') {
      return 'Error de conexión. Verifica tu conexión a internet';
    }

    if (error.name === 'TimeoutError') {
      return 'La solicitud tardó demasiado tiempo';
    }

    return error.message || 'Error inesperado';
  }

  static logError(error: any, context?: string): void {
    console.error(`[${context || 'API'}] Error:`, {
      message: error.message,
      status: error.status,
      data: error.data,
      stack: error.stack
    });
  }
}
```

### Hook para Manejo de Estados (React)

```typescript
// hooks/useApi.ts (Para React)
import { useState, useCallback } from 'react';
import { ErrorHandler } from '../utils/errorHandler';

interface ApiState<T> {
  data: T | null;
  loading: boolean;
  error: string | null;
}

export function useApi<T>() {
  const [state, setState] = useState<ApiState<T>>({
    data: null,
    loading: false,
    error: null
  });

  const execute = useCallback(async (apiCall: () => Promise<T>) => {
    setState(prev => ({ ...prev, loading: true, error: null }));
    
    try {
      const data = await apiCall();
      setState({ data, loading: false, error: null });
      return data;
    } catch (error) {
      const errorMessage = ErrorHandler.handle(error);
      ErrorHandler.logError(error, 'useApi');
      setState({ data: null, loading: false, error: errorMessage });
      throw error;
    }
  }, []);

  const reset = useCallback(() => {
    setState({ data: null, loading: false, error: null });
  }, []);

  return { ...state, execute, reset };
}
```

## Seguridad

### Mejores Prácticas de Seguridad

1. **Almacenamiento Seguro de Tokens**

```typescript
// utils/secureStorage.ts
class SecureStorage {
  private static readonly ACCESS_TOKEN_KEY = 'tesoroschoco_access_token';
  private static readonly REFRESH_TOKEN_KEY = 'tesoroschoco_refresh_token';

  static setItem(key: string, value: string): void {
    try {
      // En producción, considera usar un almacenamiento más seguro
      // como react-native-keychain para móviles
      localStorage.setItem(key, value);
    } catch (error) {
      console.error('Error storing item:', error);
    }
  }

  static getItem(key: string): string | null {
    try {
      return localStorage.getItem(key);
    } catch (error) {
      console.error('Error retrieving item:', error);
      return null;
    }
  }

  static removeItem(key: string): void {
    try {
      localStorage.removeItem(key);
    } catch (error) {
      console.error('Error removing item:', error);
    }
  }

  static setAccessToken(token: string): void {
    this.setItem(this.ACCESS_TOKEN_KEY, token);
  }

  static getAccessToken(): string | null {
    return this.getItem(this.ACCESS_TOKEN_KEY);
  }

  static setRefreshToken(token: string): void {
    this.setItem(this.REFRESH_TOKEN_KEY, token);
  }

  static getRefreshToken(): string | null {
    return this.getItem(this.REFRESH_TOKEN_KEY);
  }

  static clearTokens(): void {
    this.removeItem(this.ACCESS_TOKEN_KEY);
    this.removeItem(this.REFRESH_TOKEN_KEY);
  }
}
```

2. **Validación de Entrada**

```typescript
// utils/validators.ts
export class Validators {
  static email(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  static password(password: string): boolean {
    // Al menos 8 caracteres, una mayúscula, una minúscula y un número
    const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d@$!%*?&]{8,}$/;
    return passwordRegex.test(password);
  }

  static phone(phone: string): boolean {
    const phoneRegex = /^\+?[\d\s\-\(\)]{10,}$/;
    return phoneRegex.test(phone);
  }

  static sanitizeInput(input: string): string {
    return input.trim().replace(/[<>]/g, '');
  }
}
```

3. **CSP y Headers de Seguridad**

```typescript
// utils/security.ts
export class SecurityUtils {
  static setupCSP(): void {
    const meta = document.createElement('meta');
    meta.httpEquiv = 'Content-Security-Policy';
    meta.content = "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline';";
    document.head.appendChild(meta);
  }

  static isValidUrl(url: string): boolean {
    try {
      const parsedUrl = new URL(url);
      return parsedUrl.protocol === 'http:' || parsedUrl.protocol === 'https:';
    } catch {
      return false;
    }
  }
}
```

## Buenas Prácticas

### 1. Patrón Repository para Servicios

```typescript
// repositories/baseRepository.ts
export abstract class BaseRepository<T, K = number> {
  protected httpClient: HttpClient;
  protected baseUrl: string;

  constructor(baseUrl: string) {
    this.httpClient = HttpClient.getInstance();
    this.baseUrl = baseUrl;
  }

  async getAll(): Promise<T[]> {
    return this.httpClient.get<T[]>(this.baseUrl);
  }

  async getById(id: K): Promise<T> {
    return this.httpClient.get<T>(`${this.baseUrl}/${id}`);
  }

  async create(data: Omit<T, 'id'>): Promise<T> {
    return this.httpClient.post<T>(this.baseUrl, data);
  }

  async update(id: K, data: Partial<T>): Promise<T> {
    return this.httpClient.put<T>(`${this.baseUrl}/${id}`, data);
  }

  async delete(id: K): Promise<void> {
    return this.httpClient.delete<void>(`${this.baseUrl}/${id}`);
  }
}
```

### 2. Cache de Datos

```typescript
// utils/cache.ts
class ApiCache {
  private cache = new Map<string, { data: any; expiry: number }>();
  private defaultTTL = 5 * 60 * 1000; // 5 minutos

  set<T>(key: string, data: T, ttl = this.defaultTTL): void {
    const expiry = Date.now() + ttl;
    this.cache.set(key, { data, expiry });
  }

  get<T>(key: string): T | null {
    const item = this.cache.get(key);
    
    if (!item) {
      return null;
    }

    if (Date.now() > item.expiry) {
      this.cache.delete(key);
      return null;
    }

    return item.data;
  }

  invalidate(pattern?: string): void {
    if (!pattern) {
      this.cache.clear();
      return;
    }

    const keys = Array.from(this.cache.keys());
    keys.forEach(key => {
      if (key.includes(pattern)) {
        this.cache.delete(key);
      }
    });
  }
}

export default new ApiCache();
```

### 3. Composable para Vue 3

```typescript
// composables/useApi.ts (Para Vue 3)
import { ref, reactive } from 'vue';
import { ErrorHandler } from '../utils/errorHandler';

export function useApi<T>() {
  const data = ref<T | null>(null);
  const loading = ref(false);
  const error = ref<string | null>(null);

  const execute = async (apiCall: () => Promise<T>): Promise<T | null> => {
    loading.value = true;
    error.value = null;
    
    try {
      const result = await apiCall();
      data.value = result;
      return result;
    } catch (err) {
      const errorMessage = ErrorHandler.handle(err);
      error.value = errorMessage;
      ErrorHandler.logError(err, 'useApi');
      return null;
    } finally {
      loading.value = false;
    }
  };

  const reset = () => {
    data.value = null;
    error.value = null;
    loading.value = false;
  };

  return {
    data,
    loading,
    error,
    execute,
    reset
  };
}
```

## Ejemplos Completos

### Ejemplo React - Componente de Login

```tsx
// components/LoginForm.tsx
import React, { useState } from 'react';
import { useApi } from '../hooks/useApi';
import authService from '../services/authService';
import { Validators } from '../utils/validators';
import { AuthResponse } from '../types/auth';

const LoginForm: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [validationErrors, setValidationErrors] = useState<Record<string, string>>({});
  
  const { loading, error, execute } = useApi<AuthResponse>();

  const validateForm = (): boolean => {
    const errors: Record<string, string> = {};

    if (!email) {
      errors.email = 'El email es requerido';
    } else if (!Validators.email(email)) {
      errors.email = 'Email inválido';
    }

    if (!password) {
      errors.password = 'La contraseña es requerida';
    }

    setValidationErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    try {
      const response = await execute(() => authService.login({ email, password }));
      if (response) {
        // Redirigir al dashboard o página principal
        window.location.href = '/dashboard';
      }
    } catch (err) {
      // El error ya se maneja en useApi
    }
  };

  return (
    <form onSubmit={handleSubmit} className="login-form">
      <div className="form-group">
        <label htmlFor="email">Email:</label>
        <input
          type="email"
          id="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          className={validationErrors.email ? 'error' : ''}
        />
        {validationErrors.email && (
          <span className="error-message">{validationErrors.email}</span>
        )}
      </div>

      <div className="form-group">
        <label htmlFor="password">Contraseña:</label>
        <input
          type="password"
          id="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          className={validationErrors.password ? 'error' : ''}
        />
        {validationErrors.password && (
          <span className="error-message">{validationErrors.password}</span>
        )}
      </div>

      {error && <div className="error-message">{error}</div>}

      <button type="submit" disabled={loading}>
        {loading ? 'Iniciando sesión...' : 'Iniciar Sesión'}
      </button>
    </form>
  );
};

export default LoginForm;
```

### Ejemplo Vue 3 - Lista de Productos

```vue
<!-- components/ProductList.vue -->
<template>
  <div class="product-list">
    <div class="filters">
      <input 
        v-model="searchTerm" 
        placeholder="Buscar productos..."
        @input="debouncedSearch"
      />
      <select v-model="selectedCategory" @change="loadProducts">
        <option value="">Todas las categorías</option>
        <option 
          v-for="category in categories" 
          :key="category.id" 
          :value="category.id"
        >
          {{ category.name }}
        </option>
      </select>
    </div>

    <div v-if="loading" class="loading">
      Cargando productos...
    </div>

    <div v-else-if="error" class="error">
      {{ error }}
      <button @click="loadProducts">Reintentar</button>
    </div>

    <div v-else class="products-grid">
      <div 
        v-for="product in products" 
        :key="product.id" 
        class="product-card"
      >
        <img :src="product.image" :alt="product.name" />
        <h3>{{ product.name }}</h3>
        <p>{{ product.description }}</p>
        <div class="price">
          <span v-if="product.discountedPrice" class="discounted">
            ${{ product.discountedPrice }}
          </span>
          <span 
            :class="{ 'original': product.discountedPrice }"
          >
            ${{ product.price }}
          </span>
        </div>
        <button @click="addToCart(product.id)" :disabled="addingToCart">
          {{ addingToCart ? 'Agregando...' : 'Agregar al carrito' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { debounce } from 'lodash-es';
import { useApi } from '../composables/useApi';
import productService from '../services/productService';
import categoryService from '../services/categoryService';
import cartService from '../services/cartService';
import { ProductDto } from '../types/product';
import { CategoryDto } from '../types/category';

const searchTerm = ref('');
const selectedCategory = ref<number | ''>('');
const addingToCart = ref(false);

const { data: products, loading, error, execute } = useApi<ProductDto[]>();
const { data: categories, execute: executeCategories } = useApi<CategoryDto[]>();

const loadProducts = async () => {
  await execute(() => productService.getProducts({
    searchTerm: searchTerm.value || undefined,
    categoryId: selectedCategory.value || undefined
  }));
};

const loadCategories = async () => {
  await executeCategories(() => categoryService.getCategories());
};

const addToCart = async (productId: number) => {
  addingToCart.value = true;
  try {
    await cartService.addToCart(productId, 1);
    // Mostrar notificación de éxito
  } catch (error) {
    console.error('Error adding to cart:', error);
    // Mostrar notificación de error
  } finally {
    addingToCart.value = false;
  }
};

const debouncedSearch = debounce(() => {
  loadProducts();
}, 300);

onMounted(() => {
  loadProducts();
  loadCategories();
});

watch(selectedCategory, () => {
  loadProducts();
});
</script>
```

### Ejemplo Angular - Servicio Injectable

```typescript
// services/product.service.ts
import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject } from 'rxjs';
import { map, tap, catchError } from 'rxjs/operators';
import { HttpClient, HttpParams } from '@angular/common/http';
import { ProductDto, ProductFilters } from '../types/product';
import { ApiResponse } from '../types/api';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private readonly baseUrl = `${environment.apiUrl}/products`;
  private productsSubject = new BehaviorSubject<ProductDto[]>([]);
  
  public products$ = this.productsSubject.asObservable();

  constructor(private http: HttpClient) {}

  getProducts(filters: ProductFilters = {}): Observable<ProductDto[]> {
    let params = new HttpParams();
    
    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        params = params.set(key, value.toString());
      }
    });

    return this.http.get<ApiResponse<ProductDto[]>>(this.baseUrl, { params })
      .pipe(
        map(response => response.data),
        tap(products => this.productsSubject.next(products)),
        catchError(error => {
          console.error('Error loading products:', error);
          throw error;
        })
      );
  }

  getProductById(id: number): Observable<ProductDto> {
    return this.http.get<ApiResponse<ProductDto>>(`${this.baseUrl}/${id}`)
      .pipe(
        map(response => response.data)
      );
  }

  searchProducts(term: string): Observable<ProductDto[]> {
    return this.getProducts({ searchTerm: term });
  }
}
```

## Consideraciones de Rendimiento

### 1. Implementación de Retry Logic

```typescript
// utils/retryLogic.ts
export async function withRetry<T>(
  fn: () => Promise<T>,
  maxRetries = 3,
  delay = 1000
): Promise<T> {
  let lastError: Error;

  for (let i = 0; i <= maxRetries; i++) {
    try {
      return await fn();
    } catch (error) {
      lastError = error as Error;
      
      if (i === maxRetries) {
        throw lastError;
      }

      // Exponential backoff
      await new Promise(resolve => setTimeout(resolve, delay * Math.pow(2, i)));
    }
  }

  throw lastError!;
}
```

### 2. Throttling y Debouncing

```typescript
// utils/throttle.ts
export function throttle<T extends (...args: any[]) => any>(
  func: T,
  delay: number
): T {
  let timeoutId: NodeJS.Timeout | null = null;
  let lastExecTime = 0;

  return ((...args: Parameters<T>) => {
    const currentTime = Date.now();

    if (currentTime - lastExecTime > delay) {
      func(...args);
      lastExecTime = currentTime;
    } else {
      if (timeoutId) {
        clearTimeout(timeoutId);
      }
      
      timeoutId = setTimeout(() => {
        func(...args);
        lastExecTime = Date.now();
      }, delay - (currentTime - lastExecTime));
    }
  }) as T;
}

export function debounce<T extends (...args: any[]) => any>(
  func: T,
  delay: number
): T {
  let timeoutId: NodeJS.Timeout;

  return ((...args: Parameters<T>) => {
    clearTimeout(timeoutId);
    timeoutId = setTimeout(() => func(...args), delay);
  }) as T;
}
```

## Configuración de Desarrollo vs Producción

```typescript
// config/environment.ts
export interface Environment {
  production: boolean;
  apiUrl: string;
  timeout: number;
  retryAttempts: number;
  logLevel: 'debug' | 'info' | 'warn' | 'error';
}

export const environment: Environment = {
  production: process.env.NODE_ENV === 'production',
  apiUrl: process.env.REACT_APP_API_URL || 'http://localhost:5000/api/v1',
  timeout: 10000,
  retryAttempts: 3,
  logLevel: process.env.NODE_ENV === 'production' ? 'error' : 'debug'
};
```

## Monitoreo y Logging

```typescript
// utils/logger.ts
class Logger {
  private static instance: Logger;
  private logLevel: string;

  private constructor() {
    this.logLevel = environment.logLevel;
  }

  static getInstance(): Logger {
    if (!Logger.instance) {
      Logger.instance = new Logger();
    }
    return Logger.instance;
  }

  debug(message: string, data?: any): void {
    if (this.shouldLog('debug')) {
      console.debug(`[DEBUG] ${message}`, data);
    }
  }

  info(message: string, data?: any): void {
    if (this.shouldLog('info')) {
      console.info(`[INFO] ${message}`, data);
    }
  }

  warn(message: string, data?: any): void {
    if (this.shouldLog('warn')) {
      console.warn(`[WARN] ${message}`, data);
    }
  }

  error(message: string, error?: any): void {
    if (this.shouldLog('error')) {
      console.error(`[ERROR] ${message}`, error);
      
      // En producción, enviar a servicio de monitoreo
      if (environment.production) {
        this.sendToMonitoringService(message, error);
      }
    }
  }

  private shouldLog(level: string): boolean {
    const levels = ['debug', 'info', 'warn', 'error'];
    const currentLevelIndex = levels.indexOf(this.logLevel);
    const messageLevelIndex = levels.indexOf(level);
    
    return messageLevelIndex >= currentLevelIndex;
  }

  private async sendToMonitoringService(message: string, error: any): Promise<void> {
    // Implementar envío a servicio de monitoreo (Sentry, LogRocket, etc.)
    try {
      // await monitoringService.logError(message, error);
    } catch (err) {
      console.error('Failed to send log to monitoring service:', err);
    }
  }
}

export default Logger;
```

## Conclusión

Esta guía proporciona una base sólida para consumir la API de TesorosChocó desde cualquier aplicación frontend. Las implementaciones están diseñadas para ser:

- **Escalables**: Fácil de mantener y extender
- **Seguras**: Implementa las mejores prácticas de seguridad
- **Robustas**: Manejo adecuado de errores y casos edge
- **Reutilizables**: Código modular y bien estructurado
- **Performance**: Optimizado para aplicaciones en producción

### Próximos Pasos

1. Implementar los servicios base según tu framework frontend
2. Configurar las variables de entorno apropiadas
3. Personalizar el manejo de errores según tus necesidades
4. Implementar tests unitarios para los servicios
5. Configurar monitoreo y logging en producción

### Recursos Adicionales

- [Documentación de la API](./api-specification.md)
- [Guía de Integración .NET](./dotnet-integration.md)
- [Postman Collection](../postman/TesorosChoco.postman_collection.json) (si existe)

---

**Nota**: Esta guía asume que estás usando TypeScript. Si usas JavaScript puro, simplemente omite las anotaciones de tipos y las interfaces.
