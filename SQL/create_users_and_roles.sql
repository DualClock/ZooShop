-- =============================================
-- ZooShop — Создание таблиц Users и UserRoles
-- Запускать один раз: sqlcmd -S "(localdb)\MSSQLLocalDB" -d ZooShop -i create_users_and_roles.sql
-- =============================================

USE ZooShop;
GO

-- =============================================
-- 1. Таблица ролей (если ещё нет)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserRoles')
BEGIN
    CREATE TABLE dbo.UserRoles (
        RoleID INT IDENTITY(1,1) PRIMARY KEY,
        RoleName NVARCHAR(50) NOT NULL,
        Description NVARCHAR(200) NULL
    );

    PRINT 'Таблица UserRoles создана.';
END
ELSE
BEGIN
    PRINT 'Таблица UserRoles уже существует.';
END
GO

-- =============================================
-- 2. Заполняем роли (только если пусто)
-- =============================================
IF NOT EXISTS (SELECT * FROM dbo.UserRoles)
BEGIN
    INSERT INTO dbo.UserRoles (RoleName, Description) VALUES
    (N'Админ', N'Администратор системы'),
    (N'Сотрудник', N'Рядовой сотрудник');

    PRINT 'Роли добавлены: Админ, Сотрудник';
END
ELSE
BEGIN
    PRINT 'Роли уже заполнены.';
END
GO

-- =============================================
-- 3. Таблица пользователей (если ещё нет)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE dbo.Users (
        UserID INT IDENTITY(1,1) PRIMARY KEY,
        Login NVARCHAR(100) NOT NULL UNIQUE,
        [Password] NVARCHAR(200) NOT NULL,
        RoleID INT NOT NULL,
        EmployeeID INT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_Users_UserRole FOREIGN KEY (RoleID)
            REFERENCES dbo.UserRoles(RoleID),
        CONSTRAINT FK_Users_Employee FOREIGN KEY (EmployeeID)
            REFERENCES dbo.Employees(EmployeeID) ON DELETE SET NULL
    );

    CREATE INDEX IX_Users_Login ON dbo.Users(Login);
    PRINT 'Таблица Users создана.';
END
ELSE
BEGIN
    PRINT 'Таблица Users уже существует.';
END
GO

-- =============================================
-- 4. Заполняем пользователей (только если пусто)
-- =============================================
IF NOT EXISTS (SELECT * FROM dbo.Users)
BEGIN
    INSERT INTO dbo.Users (Login, [Password], RoleID, EmployeeID, IsActive) VALUES
    (N'maria@zooshop.ru', N'admin', 1, 3, 1),
    (N'anna@zooshop.ru',  N'123',   2, 1, 1),
    (N'ivan@zooshop.ru',  N'123',   2, 2, 1);

    PRINT 'Пользователи добавлены (3 шт).';
END
ELSE
BEGIN
    PRINT 'Таблица Users уже заполнена. Пропускаем.';
END
GO

-- =============================================
-- 5. Проверка
-- =============================================
PRINT '';
PRINT '=== UserRoles ===';
SELECT RoleID, RoleName, Description FROM dbo.UserRoles;

PRINT '';
PRINT '=== Users ===';
SELECT u.UserID, u.Login, u.[Password], r.RoleName, u.EmployeeID, u.IsActive
FROM dbo.Users u
JOIN dbo.UserRoles r ON u.RoleID = r.RoleID;
GO
