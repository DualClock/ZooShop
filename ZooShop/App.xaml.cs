using System.Windows;
using System.IO;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ZooShop.Data;

namespace ZooShop;

public partial class App : Application
{
    private static readonly string AppLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app-debug.log");

    private static void LogApp(string message)
    {
        try
        {
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}\n";
            File.AppendAllText(AppLogPath, line);
            System.Diagnostics.Debug.WriteLine(line);
        }
        catch { }
    }
    protected override void OnStartup(StartupEventArgs e)
    {
        LogApp("=== ЗАПУСК ПРИЛОЖЕНИЯ ===");
        
        // Глобальный перехват всех исключений
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            LogApp($"UnhandledException: {(args.ExceptionObject as Exception)?.Message}");
            LogException(args.ExceptionObject as Exception, "UnhandledException");
        };

        DispatcherUnhandledException += (s, args) =>
        {
            LogApp($"DispatcherUnhandledException: {args.Exception.Message}");
            LogException(args.Exception, "DispatcherUnhandledException");
            args.Handled = true;
            MessageBox.Show(
                $"Произошла ошибка: {args.Exception.Message}\n\nПриложение продолжит работу.",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        };

        base.OnStartup(e);
        LogApp("Базовый OnStartup вызван");

        // КРИТИЧЕСКИ ВАЖНО: отключаем автоматическое завершение приложения
        // По умолчанию ShutdownMode=OnLastWindowClose — когда все окна закрыты, WPF завершает приложение.
        // У нас цикл входа: LoginWindow закрывается -> MainWindow открывается -> LoginWindow уже закрыт -> WPF думает что все окна закрыты -> убивает MainWindow
        Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        LogApp("ShutdownMode установлен в OnExplicitShutdown");

        // Инициализация базы данных
        LogApp("Начало инициализации БД...");
        InitializeDatabase();
        LogApp("Инициализация БД завершена");

        // Цикл входа: показываем LoginWindow, если успешно — MainWindow, и так по кругу
        LogApp("Начало цикла входа...");
        int loopCount = 0;
        while (true)
        {
            loopCount++;
            LogApp($"--- Итерация цикла #{loopCount} ---");
            
            var loginWindow = new Views.LoginWindow();
            LogApp("Показываю LoginWindow.ShowDialog()...");
            var loginResult = loginWindow.ShowDialog();
            LogApp($"LoginWindow.ShowDialog() вернул: {loginResult}");
            LogApp($"SessionService.IsAuthenticated={SessionService.IsAuthenticated}");
            LogApp($"SessionService.CurrentUser: {SessionService.CurrentUser?.Login ?? "null"}");

            if (loginResult != true)
            {
                LogApp("loginResult != true — выход из цикла (пользователь закрыл окно входа)");
                // Пользователь закрыл окно входа — выходим из приложения
                break;
            }

            LogApp("Успешный вход — показываю MainWindow");
            // Успешный вход — показываем главное окно
            try
            {
                var mainWindow = new MainWindow();
                // ВАЖНО: назначаем MainWindow как главное окно приложения
                // иначе WPF закроет приложение, т.к. все окна закрыты (ShutdownMode=OnLastWindowClose)
                MainWindow = mainWindow;
                LogApp("MainWindow назначен как Application.MainWindow");
                LogApp("MainWindow создан, показываю ShowDialog()...");
                mainWindow.ShowDialog();
                LogApp("MainWindow.ShowDialog() закрылся");
                MainWindow = null; // Сбрасываем, чтобы цикл мог продолжиться
            }
            catch (Exception mainEx)
            {
                LogApp($"КРИТИЧЕСКАЯ ОШИБКА при создании/показе MainWindow: {mainEx.GetType().Name}: {mainEx.Message}");
                LogApp($"StackTrace: {mainEx.StackTrace}");
                MessageBox.Show(
                    $"Ошибка при открытии главного окна:\n{mainEx.GetType().Name}: {mainEx.Message}\n\nПодробности: {mainEx.StackTrace}",
                    "Критическая ошибка ZooShop",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }

            LogApp($"После MainWindow: SessionService.IsAuthenticated={SessionService.IsAuthenticated}");
            
            // MainWindow закрылся (выход) — цикл повторяется, показываем LoginWindow снова
            if (!SessionService.IsAuthenticated)
            {
                LogApp("Был logout — продолжаю цикл");
                // Был logout — продолжаем цикл
                continue;
            }

            LogApp("MainWindow закрылся по другой причине — выход из цикла");
            // MainWindow закрылся по другой причине — выходим
            break;
        }

        LogApp("Выход из цикла входа, вызываю Shutdown()");
        Shutdown();
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
                $"Убедитесь, что SQL Server запущен ({DbConfig.ServerName})",
                "ZooShop - Ошибка БД",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        // Проверяем и исправляем роли пользователей (отдельный контекст)
        try
        {
            using var checkDb = new ZooShopDbContext();
            EnsureUsersAndRolesExist(checkDb);
        }
        catch (Exception ex)
        {
            LogApp($"Ошибка проверки пользователей: {ex.Message}");
        }
    }

    private static void EnsureUsersAndRolesExist(ZooShopDbContext db)
    {
        try
        {
            // 1. Проверяем/создаём роли
            var adminRole = db.UserRoles.FirstOrDefault(r => r.RoleName == "Админ");
            if (adminRole == null)
            {
                adminRole = new ZooShop.Models.UserRole { RoleName = "Админ", Description = "Администратор системы" };
                db.UserRoles.Add(adminRole);
                db.SaveChanges();
                LogApp("Роль 'Админ' создана");
            }

            var employeeRole = db.UserRoles.FirstOrDefault(r => r.RoleName == "Сотрудник");
            if (employeeRole == null)
            {
                employeeRole = new ZooShop.Models.UserRole { RoleName = "Сотрудник", Description = "Рядовой сотрудник" };
                db.UserRoles.Add(employeeRole);
                db.SaveChanges();
                LogApp("Роль 'Сотрудник' создана");
            }

            // 2. Проверяем/создаём пользователей
            EnsureUserExists(db, "maria@zooshop.ru", "admin", adminRole.RoleID, "Мария", "Админ");
            EnsureUserExists(db, "anna@zooshop.ru", "123", employeeRole.RoleID, "Анна", "Сотрудник");
            EnsureUserExists(db, "ivan@zooshop.ru", "123", employeeRole.RoleID, "Иван", "Сотрудник");

            // 3. Логируем всех пользователей
            var users = db.Users.Include(u => u.Role).ToList();
            LogApp($"Пользователи в БД: {users.Count}");
            foreach (var u in users)
            {
                LogApp($"  - {u.Login} | Роль: {u.Role?.RoleName ?? "null"} | IsActive: {u.IsActive}");
            }
        }
        catch (Exception ex)
        {
            LogApp($"Ошибка проверки пользователей: {ex.Message}");
        }
    }

    private static void EnsureUserExists(ZooShopDbContext db, string login, string password, int roleId, string firstName, string expectedRole)
    {
        var user = db.Users.FirstOrDefault(u => u.Login == login);
        if (user == null)
        {
            // Ищем сотрудника по имени
            var employee = db.Employees.FirstOrDefault(e => e.FirstName == firstName);
            
            user = new ZooShop.Models.User
            {
                Login = login,
                Password = password,
                RoleID = roleId,
                EmployeeID = employee?.EmployeeID,
                IsActive = true
            };
            db.Users.Add(user);
            db.SaveChanges();
            LogApp($"Пользователь {login} добавлен (Роль: {expectedRole}, IsActive=true)");
        }
        else
        {
            // Убедимся что роль правильная и пользователь АКТИВЕН
            bool changed = false;
            if (user.RoleID != roleId)
            {
                user.RoleID = roleId;
                changed = true;
            }
            if (!user.IsActive)
            {
                user.IsActive = true;
                changed = true;
            }
            if (changed)
            {
                db.SaveChanges();
                LogApp($"Пользователь {login} обновлён (Роль: {expectedRole}, IsActive=true)");
            }
        }
    }

    private static void EnsureDatabaseExists()
    {
        var masterConn = DbConfig.MasterConnectionString;
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
