-- Создание представлений vw_CurrentStock и vw_TodaySales
-- Этот скрипт выполняется после создания БД через EF Core миграцию

IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vw_CurrentStock')
BEGIN
    EXEC('
    CREATE VIEW vw_CurrentStock AS
    SELECT
        p.ProductID,
        p.ProductName,
        w.WarehouseID,
        w.WarehouseName,
        SUM(CASE
            WHEN im.MovementType = N''IN'' THEN im.Quantity
            WHEN im.MovementType = N''OUT'' THEN -im.Quantity
            WHEN im.MovementType = N''WRITE_OFF'' THEN -im.Quantity
            WHEN im.MovementType = N''TRANSFER'' THEN -im.Quantity
            ELSE 0
        END) as CurrentQuantity
    FROM Products p
    CROSS JOIN Warehouses w
    LEFT JOIN InventoryMovements im ON p.ProductID = im.ProductID AND w.WarehouseID = im.WarehouseID
    GROUP BY p.ProductID, p.ProductName, w.WarehouseID, w.WarehouseName
    HAVING SUM(CASE
            WHEN im.MovementType = N''IN'' THEN im.Quantity
            WHEN im.MovementType = N''OUT'' THEN -im.Quantity
            WHEN im.MovementType = N''WRITE_OFF'' THEN -im.Quantity
            WHEN im.MovementType = N''TRANSFER'' THEN -im.Quantity
            ELSE 0
        END) <> 0 OR SUM(CASE
            WHEN im.MovementType = N''IN'' THEN im.Quantity
            WHEN im.MovementType = N''OUT'' THEN -im.Quantity
            WHEN im.MovementType = N''WRITE_OFF'' THEN -im.Quantity
            WHEN im.MovementType = N''TRANSFER'' THEN -im.Quantity
            ELSE 0
        END) IS NULL
    ');
END
GO

IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vw_TodaySales')
BEGIN
    EXEC('
    CREATE VIEW vw_TodaySales AS
    SELECT
        COUNT(*) as TotalSales,
        SUM(TotalAmount) as TotalRevenue,
        SUM(TotalAmount - DiscountAmount) as NetRevenue,
        AVG(TotalAmount) as AvgCheck
    FROM Sales
    WHERE CAST(SaleDate AS DATE) = CAST(GETDATE() AS DATE)
    ');
END
GO
