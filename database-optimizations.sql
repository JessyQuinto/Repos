-- 🗄️ Optimizaciones de Base de Datos - TesorosChoco
-- Ejecutar estos scripts para mejorar el rendimiento

-- ================================
-- 1. ÍNDICES PARA PRODUCTOS
-- ================================

-- Índice compuesto para búsquedas por categoría y estado
CREATE NONCLUSTERED INDEX IX_Products_CategoryId_Status_Featured
ON Products (CategoryId, Status, Featured)
INCLUDE (Id, Name, Price, ImageUrl);

-- Índice para búsquedas por productor
CREATE NONCLUSTERED INDEX IX_Products_ProducerId_Status
ON Products (ProducerId, Status)
INCLUDE (Id, Name, Price);

-- Índice para búsquedas de texto en nombre y descripción
CREATE NONCLUSTERED INDEX IX_Products_Name_Description
ON Products (Name, Description);

-- ================================
-- 2. ÍNDICES PARA CARRITO
-- ================================

-- Índice para obtener carrito por usuario
CREATE NONCLUSTERED INDEX IX_Carts_UserId
ON Carts (UserId)
INCLUDE (Id, CreatedAt, UpdatedAt);

-- Índice para items del carrito
CREATE NONCLUSTERED INDEX IX_CartItems_CartId_ProductId
ON CartItems (CartId, ProductId)
INCLUDE (Quantity, Price);

-- ================================
-- 3. ÍNDICES PARA ÓRDENES
-- ================================

-- Índice para órdenes por usuario y estado
CREATE NONCLUSTERED INDEX IX_Orders_UserId_Status_CreatedAt
ON Orders (UserId, Status, CreatedAt DESC)
INCLUDE (Id, Total);

-- Índice para items de orden
CREATE NONCLUSTERED INDEX IX_OrderItems_OrderId_ProductId
ON OrderItems (OrderId, ProductId)
INCLUDE (Quantity, Price);

-- ================================
-- 4. ÍNDICES PARA RESERVAS DE STOCK
-- ================================

-- Índice crítico para reservas activas por producto
CREATE NONCLUSTERED INDEX IX_StockReservations_ProductId_IsActive_ExpiresAt
ON StockReservations (ProductId, IsActive, ExpiresAt)
INCLUDE (UserId, Quantity);

-- Índice para limpiar reservas expiradas
CREATE NONCLUSTERED INDEX IX_StockReservations_IsActive_ExpiresAt
ON StockReservations (IsActive, ExpiresAt)
WHERE IsActive = 1;

-- Índice para reservas por usuario
CREATE NONCLUSTERED INDEX IX_StockReservations_UserId_IsActive
ON StockReservations (UserId, IsActive)
INCLUDE (ProductId, Quantity, ExpiresAt);

-- ================================
-- 5. ESTADÍSTICAS Y MANTENIMIENTO
-- ================================

-- Actualizar estadísticas de todas las tablas
UPDATE STATISTICS Products;
UPDATE STATISTICS Categories;
UPDATE STATISTICS Producers;
UPDATE STATISTICS Carts;
UPDATE STATISTICS CartItems;
UPDATE STATISTICS Orders;
UPDATE STATISTICS OrderItems;
UPDATE STATISTICS StockReservations;

-- ================================
-- 6. CONSULTAS DE VERIFICACIÓN
-- ================================

-- Verificar índices creados
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    ds.name AS FileGroup
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
INNER JOIN sys.data_spaces ds ON i.data_space_id = ds.data_space_id
WHERE t.name IN ('Products', 'Categories', 'Producers', 'Carts', 'CartItems', 'Orders', 'OrderItems', 'StockReservations')
    AND i.type > 0
ORDER BY t.name, i.name;

-- Verificar fragmentación de índices
SELECT 
    t.name AS TableName,
    i.name AS IndexName,
    ps.avg_fragmentation_in_percent,
    ps.page_count
FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ps
INNER JOIN sys.indexes i ON ps.object_id = i.object_id AND ps.index_id = i.index_id
INNER JOIN sys.tables t ON i.object_id = t.object_id
WHERE ps.avg_fragmentation_in_percent > 10
    AND ps.page_count > 1000
ORDER BY ps.avg_fragmentation_in_percent DESC;

-- ================================
-- 7. CONFIGURACIONES DE RENDIMIENTO
-- ================================

-- Configurar opciones de base de datos para mejor rendimiento
ALTER DATABASE TesorosChocoDB SET AUTO_UPDATE_STATISTICS_ASYNC ON;
ALTER DATABASE TesorosChocoDB SET PAGE_VERIFY CHECKSUM;
ALTER DATABASE TesorosChocoDB SET TARGET_RECOVERY_TIME = 60 SECONDS;

-- ================================
-- 8. JOBS DE MANTENIMIENTO
-- ================================

-- Script para limpiar reservas expiradas (ejecutar desde la aplicación)
/*
DELETE FROM StockReservations 
WHERE IsActive = 1 
    AND ExpiresAt < GETUTCDATE();
*/

-- ================================
-- 9. MONITOREO DE RENDIMIENTO
-- ================================

-- Vista para monitorear reservas activas
CREATE VIEW v_ActiveReservations AS
SELECT 
    sr.ProductId,
    p.Name AS ProductName,
    COUNT(*) AS ActiveReservations,
    SUM(sr.Quantity) AS TotalReservedQuantity,
    p.Stock AS CurrentStock,
    (p.Stock - SUM(sr.Quantity)) AS AvailableStock
FROM StockReservations sr
INNER JOIN Products p ON sr.ProductId = p.Id
WHERE sr.IsActive = 1 AND sr.ExpiresAt > GETUTCDATE()
GROUP BY sr.ProductId, p.Name, p.Stock;

-- Vista para productos más vendidos
CREATE VIEW v_TopSellingProducts AS
SELECT 
    p.Id,
    p.Name,
    COUNT(oi.Id) AS OrderCount,
    SUM(oi.Quantity) AS TotalSold,
    SUM(oi.Quantity * oi.Price) AS TotalRevenue
FROM Products p
INNER JOIN OrderItems oi ON p.Id = oi.ProductId
INNER JOIN Orders o ON oi.OrderId = o.Id
WHERE o.Status != 0 -- Excluir órdenes canceladas
GROUP BY p.Id, p.Name;

-- ================================
-- 10. BACKUP Y RECOVERY
-- ================================

-- Script de backup (ajustar rutas según sea necesario)
/*
BACKUP DATABASE TesorosChocoDB 
TO DISK = 'C:\Backups\TesorosChocoDB_Full.bak'
WITH FORMAT, INIT, SKIP, NOREWIND, NOUNLOAD, COMPRESSION, STATS = 10;
*/

PRINT 'Optimizaciones de base de datos aplicadas correctamente ✅';
PRINT 'Recuerda configurar trabajos de mantenimiento programados 📅';
