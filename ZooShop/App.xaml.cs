using System.Windows;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ZooShop.Data;

namespace ZooShop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Глобальный перехват всех исключений
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            LogException(args.ExceptionObject as Exception, "UnhandledException");
        };

        DispatcherUnhandledException += (s, args) =>
        {
            LogException(args.Exception, "DispatcherUnhandledException");
            args.Handled = true;
            MessageBox.Show(
                $"Произошла ошибка: {args.Exception.Message}\n\nПриложение продолжит работу.",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        };

        base.OnStartup(e);

        // Инициализация базы данных
        InitializeDatabase();

        // Запуск главного окна
        new MainWindow().Show();
    }

    private static void LogException(Exception? ex, string source)
    {
        try
        {
            var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            File.AppendAllText(logPath,
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{source}] {ex?.GetType().Name}: {ex?.Message}\n{ex?.StackTrace}\n{'=',80}\n");
        }
        catch { }
    }

    private static void InitializeDatabase()
    {
        try
        {
            // Создаём БД если не существует
            EnsureDatabaseExists();

            using var db = new ZooShopDbContext();

            // Применяем миграцию
            db.Database.Migrate();

            // Создаём представления
            CreateViews(db);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка инициализации: {ex.Message}");
            MessageBox.Show(
                $"Ошибка инициализации базы данных:\n{ex.Message}\n\n" +
                "Убедитесь, что SQL Server запущен и доступен через (localdb)\\MSSQLLocalDB",
                "ZooShop - Ошибка БД",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static void EnsureDatabaseExists()
    {
        var masterConn = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";
        using var conn = new SqlConnection(masterConn);
        conn.Open();

        var cmdText = @"
            IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ZooShop')
            BEGIN
                CREATE DATABASE ZooShop COLLATE Cyrillic_General_100_CI_AS_SC_UTF8;
            END";

        using var cmd = new SqlCommand(cmdText, conn);
        cmd.ExecuteNonQuery();
    }

    private static void CreateViews(ZooShopDbContext db)
    {
        try
        {
            using var conn = db.Database.GetDbConnection();
            conn.Open();

            // vw_CurrentStock
            var checkView = "SELECT COUNT(*) FROM sys.views WHERE name = 'vw_CurrentStock'";
            using (var cmd = new SqlCommand(checkView, (SqlConnection)conn))
            {
                var exists = (int)cmd.ExecuteScalar();
                if (exists == 0)
                {
                    var sql = @"
                    CREATE VIEW vw_CurrentStock AS
                    SELECT
                        p.ProductID,
                        p.ProductName,
                        w.WarehouseID,
                        w.WarehouseName,
                        SUM(CASE
                            WHEN im.MovementType = N'IN' THEN im.Quantity
                            WHEN im.MovementType = N'OUT' THEN -im.Quantity
                            WHEN im.MovementType = N'WRITE_OFF' THEN -im.Quantity
                            WHEN im.MovementType = N'TRANSFER' THEN -im.Quantity
                            ELSE 0
                        END) as CurrentQuantity
                    FROM Products p
                    CROSS JOIN Warehouses w
                    LEFT JOIN InventoryMovements im ON p.ProductID = im.ProductID AND w.WarehouseID = im.WarehouseID
                    GROUP BY p.ProductID, p.ProductName, w.WarehouseID, w.WarehouseName
                    HAVING SUM(CASE
                            WHEN im.MovementType = N'IN' THEN im.Quantity
                            WHEN im.MovementType = N'OUT' THEN -im.Quantity
                            WHEN im.MovementType = N'WRITE_OFF' THEN -im.Quantity
                            WHEN im.MovementType = N'TRANSFER' THEN -im.Quantity
                            ELSE 0
                        END) <> 0 OR SUM(CASE
                            WHEN im.MovementType = N'IN' THEN im.Quantity
                            WHEN im.MovementType = N'OUT' THEN -im.Quantity
                            WHEN im.MovementType = N'WRITE_OFF' THEN -im.Quantity
                            WHEN im.MovementType = N'TRANSFER' THEN -im.Quantity
                            ELSE 0
                        END) IS NULL";

                    using var createCmd = new SqlCommand(sql, (SqlConnection)conn);
                    createCmd.ExecuteNonQuery();
                }
            }

            // vw_TodaySales
            checkView = "SELECT COUNT(*) FROM sys.views WHERE name = 'vw_TodaySales'";
            using (var cmd = new SqlCommand(checkView, (SqlConnection)conn))
            {
                var exists = (int)cmd.ExecuteScalar();
                if (exists == 0)
                {
                    var sql = @"
                    CREATE VIEW vw_TodaySales AS
                    SELECT
                        COUNT(*) as TotalSales,
                        SUM(TotalAmount) as TotalRevenue,
                        SUM(TotalAmount - DiscountAmount) as NetRevenue,
                        AVG(TotalAmount) as AvgCheck
                    FROM Sales
                    WHERE CAST(SaleDate AS DATE) = CAST(GETDATE() AS DATE)";

                    using var createCmd = new SqlCommand(sql, (SqlConnection)conn);
                    createCmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка создания представлений: {ex.Message}");
        }
    }
}
